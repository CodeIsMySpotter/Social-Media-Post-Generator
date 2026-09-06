using System.Net.Http.Json;
using GeneratorService.Core.Global.Database;
using GeneratorService.Core.User.Repositories.Models;
using GeneratorService.Core.User.Requests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using Xunit;

namespace GeneratorService.Tests;

public class UserProfileEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly SqliteConnection _connection;

    public UserProfileEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var dbConnectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbConnection));
                if (dbConnectionDescriptor != null)
                {
                    services.Remove(dbConnectionDescriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });
            });
        });

        _client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }

    [Fact]
    public async Task GetProfile_WhenAuthenticated_ReturnsProfileWithRandomName()
    {
        // Arrange
        var email = $"test_profile_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(email, "SuperSecurePassword123!");

        // Act - Register user (this should also create the profile)
        var registerResponse = await _client.PostAsJsonAsync("/register", request);
        registerResponse.EnsureSuccessStatusCode();

        // Extract cookie
        var cookies = registerResponse.Headers.GetValues("Set-Cookie");
        var tokenCookie = cookies.FirstOrDefault(c => c.StartsWith("access_token="));
        Assert.NotNull(tokenCookie);

        // Extract just the value of the cookie before the semicolon
        var tokenValue = tokenCookie.Split(';')[0];

        // Prepare request to /profile
        var profileRequest = new HttpRequestMessage(HttpMethod.Get, "/profile");
        profileRequest.Headers.Add("Cookie", tokenValue);

        // Act - Get profile
        var profileResponse = await _client.SendAsync(profileRequest);

        // Assert
        if (!profileResponse.IsSuccessStatusCode)
        {
            var error = await profileResponse.Content.ReadAsStringAsync();
            throw new Exception($"Status Code: {profileResponse.StatusCode}, Error: {error}");
        }

        var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileModel>();
        Assert.NotNull(profile);
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.False(string.IsNullOrWhiteSpace(profile.Name), "Profile name should not be empty (should be randomly generated).");
    }

    [Fact]
    public async Task GetProfile_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/profile");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WhenAuthenticated_UpdatesSuccessfully()
    {
        // Arrange
        var email = $"test_profile2_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(email, "SuperSecurePassword123!");
        var registerResponse = await _client.PostAsJsonAsync("/register", request);
        registerResponse.EnsureSuccessStatusCode();

        var tokenCookie = registerResponse.Headers.GetValues("Set-Cookie").FirstOrDefault(c => c.StartsWith("access_token="));
        var tokenValue = tokenCookie!.Split(';')[0];

        var updateRequestMsg = new HttpRequestMessage(HttpMethod.Put, "/profile");
        updateRequestMsg.Headers.Add("Cookie", tokenValue);
        updateRequestMsg.Content = JsonContent.Create(new UpdateProfileRequest { Name = "NewName", AvatarUrl = "http://avatar.com/1.png" });

        // Act
        var updateResponse = await _client.SendAsync(updateRequestMsg);

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var content = await updateResponse.Content.ReadAsStringAsync();
        Assert.Contains("Profile updated successfully", content);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
