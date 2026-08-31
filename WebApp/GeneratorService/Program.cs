using System.Text;
using GeneratorService.Core.Global.Database;
using GeneratorService.Core.User;
using GeneratorService.Core.User.Configuration;
using GeneratorService.Core.User.Repositories;
using GeneratorService.Core.User.Services;
using GeneratorService.Core.User.Services.Subservices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


using GeneratorService.Core.Global.ExceptionHandlers;
using GeneratorService.Core.User.ExceptionHandlers;

void SetupServices(WebApplicationBuilder builder) {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                           ?? "Data Source=app.db";

    if (!builder.Environment.IsDevelopment())
    {
        var envConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            connectionString = envConnectionString;
        }
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));

    builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>();
    builder.Services.AddScoped<IUserAuthService, UserAuthService>();
    builder.Services.AddScoped<IUserJwtService, UserJwtService>();

    // Exception Handlers
    builder.Services.AddExceptionHandler<UserAuthExceptionHandler>();
    builder.Services.AddExceptionHandler<DefaultGlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddAuthorization();
    builder.Services.AddOpenApi();

}

void SetupJwtLogic(WebApplicationBuilder builder) {
    builder.Services.Configure<AuthServiceConfiguration>(builder.Configuration.GetSection("Jwt"));
    builder.Services.AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options => {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<AuthServiceConfiguration>();
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? ""))
        };

        options.Events = new JwtBearerEvents {
            OnMessageReceived = context => {
                if (context.Request.Cookies.ContainsKey("access_token")) {
                    context.Token = context.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            }
        };
    });
}


void ConfigurePipeline(WebApplication app) {
    using (var scope = app.Services.CreateScope()) {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
    }

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment()) {
        app.MapOpenApi();
    }
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.RegisterUserAuthRoutes();
}

var builder = WebApplication.CreateBuilder(args);
SetupJwtLogic(builder);
SetupServices(builder);

var app = builder.Build();
ConfigurePipeline(app);
app.Run();

public partial class Program { }


