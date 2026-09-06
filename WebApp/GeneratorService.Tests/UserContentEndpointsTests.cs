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

public class UserContentEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly SqliteConnection _connection;

    public UserContentEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
                if (dbConnectionDescriptor != null) services.Remove(dbConnectionDescriptor);

                services.AddDbContext<AppDbContext>(options => { options.UseSqlite(_connection); });
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

    private async Task<string> AuthenticateAsync()
    {
        var email = $"test_content_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(email, "SuperSecurePassword123!");
        var registerResponse = await _client.PostAsJsonAsync("/register", request);
        registerResponse.EnsureSuccessStatusCode();

        var tokenCookie = registerResponse.Headers.GetValues("Set-Cookie").FirstOrDefault(c => c.StartsWith("access_token="));
        return tokenCookie!.Split(';')[0];
    }

    [Fact]
    public async Task ContentCrudOperations_WorkCorrectly()
    {
        // 1. Authenticate
        var token = await AuthenticateAsync();

        // 2. Create content
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/content");
        createRequest.Headers.Add("Cookie", token);
        createRequest.Content = JsonContent.Create(new CreateContentRequest { ContentName = "TestPost1", ContentCode = "<h1>Test</h1>" });
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        // 3. Get all contents
        var getAllRequest = new HttpRequestMessage(HttpMethod.Get, "/content");
        getAllRequest.Headers.Add("Cookie", token);
        var getAllResponse = await _client.SendAsync(getAllRequest);
        getAllResponse.EnsureSuccessStatusCode();
        var contents = await getAllResponse.Content.ReadFromJsonAsync<List<UserContentModel>>();
        Assert.NotNull(contents);
        Assert.Single(contents);
        Assert.Equal("TestPost1", contents[0].ContentName);

        // 4. Update content
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/content/TestPost1");
        updateRequest.Headers.Add("Cookie", token);
        updateRequest.Content = JsonContent.Create(new UpdateContentRequest { ContentCode = "<h2>Updated</h2>" });
        var updateResponse = await _client.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        // 5. Get single content to verify update
        var getSingleRequest = new HttpRequestMessage(HttpMethod.Get, "/content/TestPost1");
        getSingleRequest.Headers.Add("Cookie", token);
        var getSingleResponse = await _client.SendAsync(getSingleRequest);
        getSingleResponse.EnsureSuccessStatusCode();
        var content = await getSingleResponse.Content.ReadFromJsonAsync<UserContentModel>();
        Assert.NotNull(content);
        Assert.Equal("<h2>Updated</h2>", content.ContentCode);

        // 6. Delete content
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/content/TestPost1");
        deleteRequest.Headers.Add("Cookie", token);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        deleteResponse.EnsureSuccessStatusCode();
        
        // Verify deletion
        var getAgainRequest = new HttpRequestMessage(HttpMethod.Get, "/content/TestPost1");
        getAgainRequest.Headers.Add("Cookie", token);
        var getAgainResponse = await _client.SendAsync(getAgainRequest);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getAgainResponse.StatusCode);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
