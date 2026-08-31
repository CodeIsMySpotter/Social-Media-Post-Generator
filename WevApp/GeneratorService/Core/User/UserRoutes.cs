using GeneratorService.Core.User.Requests;
using GeneratorService.Core.User.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GeneratorService.Core.User;

public static class UserAuthRoutes {
    public static void RegisterUserAuthRoutes(this WebApplication app) {
        app.MapPost("/register", async (RegisterRequest request, IUserAuthService service, HttpContext context) => {
            var token = await service.RegisterAsync(request);
            context.Response.Cookies.Append("access_token", token, new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        });

        app.MapPost("/login", async (LoginRequest request, IUserAuthService service, HttpContext context) => {
            var token = await service.LoginAsync(request);
            context.Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Wymaga HTTPS
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Results.Ok(new { message = "Successfully logged in!" });
        });

        app.MapPost("/logout", async (HttpContext context) => {
            context.Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Results.Ok(new { message = "Successfully logged out!" });
        });
    }
}


public static class UserProfileRoutes {
    public static void RegisterUserProfileRoutes(this IEndpointRouteBuilder app) {

    }        
}


public static class UserContentRoutes {
    public static void RegisterUserContentRoutes(this IEndpointRouteBuilder app) {
        
    }        
}
