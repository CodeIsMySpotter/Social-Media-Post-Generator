using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using GeneratorService.Core.User.Requests;
using Microsoft.Extensions.DependencyInjection;
using GeneratorService.Core.Global.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;
using GeneratorService.Core.User.Repositories.Models;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace GeneratorService.Tests;

public class UserAuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly SqliteConnection _connection;

    public UserAuthEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task Register_WithValidData_ReturnsOkAndSetsCookie()
    {
        // Arrange
        var email = $"test_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(email, "SuperSecurePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/register", request);

        // Assert
        if (!response.IsSuccessStatusCode) {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Status Code: {response.StatusCode}, Error: {error}");
        }
        
        Assert.True(response.Headers.Contains("Set-Cookie"), "Response should contain Set-Cookie header.");
        var cookies = response.Headers.GetValues("Set-Cookie");
        Assert.Contains(cookies, c => c.StartsWith("access_token="));
    }

    [Fact]
    public async Task Login_WithValidData_ReturnsOkAndSetsCookie()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid()}@example.com";
        var password = "SuperSecurePassword123!";
        var registerRequest = new RegisterRequest(email, password);
        await _client.PostAsJsonAsync("/register", registerRequest);

        var loginRequest = new LoginRequest(email, password);

        // Act
        var response = await _client.PostAsJsonAsync("/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        Assert.True(response.Headers.Contains("Set-Cookie"), "Response should contain Set-Cookie header.");
        var cookies = response.Headers.GetValues("Set-Cookie");
        Assert.Contains(cookies, c => c.StartsWith("access_token="));
    }

    [Fact]
    public async Task Logout_RemovesCookie()
    {
        // Act
        var response = await _client.PostAsync("/logout", null);

        // Assert
        response.EnsureSuccessStatusCode();

        Assert.True(response.Headers.Contains("Set-Cookie"), "Response should contain Set-Cookie header.");
        var cookies = response.Headers.GetValues("Set-Cookie");
        Assert.Contains(cookies, c => c.Contains("access_token=") && c.Contains("expires="));
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}

