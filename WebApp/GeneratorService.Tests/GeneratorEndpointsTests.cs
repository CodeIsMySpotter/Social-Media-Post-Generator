using System.Net.Http.Json;
using GeneratorService.Core.Global.Database;
using GeneratorService.Core.User.Requests;
using GeneratorService.Core.Generator.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using Moq;
using Xunit;

namespace GeneratorService.Tests;

public class GeneratorEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly SqliteConnection _connection;
    private readonly Mock<IMediaGeneratorService> _mediaGeneratorMock;

    public GeneratorEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _mediaGeneratorMock = new Mock<IMediaGeneratorService>();
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
                if (dbConnectionDescriptor != null) services.Remove(dbConnectionDescriptor);

                services.AddDbContext<AppDbContext>(options => { options.UseSqlite(_connection); });

                var mediaGenDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediaGeneratorService));
                if (mediaGenDescriptor != null) services.Remove(mediaGenDescriptor);
                
                services.AddScoped<IMediaGeneratorService>(_ => _mediaGeneratorMock.Object);
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
        var email = $"test_gen_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(email, "SuperSecurePassword123!");
        var registerResponse = await _client.PostAsJsonAsync("/register", request);
        registerResponse.EnsureSuccessStatusCode();

        var tokenCookie = registerResponse.Headers.GetValues("Set-Cookie").FirstOrDefault(c => c.StartsWith("access_token="));
        return tokenCookie!.Split(';')[0];
    }

    [Fact]
    public async Task CreateContent_And_GenerateImage_Success()
    {
        // Arrange
        var token = await AuthenticateAsync();
        
        // Create content
        var createRequestMsg = new HttpRequestMessage(HttpMethod.Post, "/content");
        createRequestMsg.Headers.Add("Cookie", token);
        createRequestMsg.Content = JsonContent.Create(new CreateContentRequest { ContentName = "MyFirstPost", ContentCode = "<h1>Hello</h1>" });
        var createResponse = await _client.SendAsync(createRequestMsg);
        createResponse.EnsureSuccessStatusCode();

        // Setup mock for image generation
        var dummyBytes = new byte[] { 0x01, 0x02, 0x03 };
        _mediaGeneratorMock.Setup(m => m.GenerateImageAsync(It.IsAny<string>())).ReturnsAsync(dummyBytes);

        // Act - Generate Image
        var genRequestMsg = new HttpRequestMessage(HttpMethod.Post, "/generate/image/MyFirstPost");
        genRequestMsg.Headers.Add("Cookie", token);
        var genResponse = await _client.SendAsync(genRequestMsg);

        // Assert
        genResponse.EnsureSuccessStatusCode();
        var bytes = await genResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(dummyBytes, bytes);
        _mediaGeneratorMock.Verify(m => m.GenerateImageAsync(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task GenerateImage_ReturnsNotFound_WhenContentDoesNotExist()
    {
        // Arrange
        var token = await AuthenticateAsync();

        // Act
        var genRequestMsg = new HttpRequestMessage(HttpMethod.Post, "/generate/image/NonExistentPost");
        genRequestMsg.Headers.Add("Cookie", token);
        var genResponse = await _client.SendAsync(genRequestMsg);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, genResponse.StatusCode);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
