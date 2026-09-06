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
                Secure = true,
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
        var group = app.MapGroup("/profile").RequireAuthorization();

        group.MapGet("/", async (IUserProfileService profileService, HttpContext context) => {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var profile = await profileService.GetProfileByIdAsync(userId.Value);
            return profile == null ? Results.NotFound(new { message = "Profile not found." }) : Results.Ok(profile);
        });

        group.MapPut("/", async (UpdateProfileRequest request, IUserProfileService profileService, HttpContext context) => {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            await profileService.UpdateProfileAsync(userId.Value, request.Name, request.AvatarUrl);
            return Results.Ok(new { message = "Profile updated successfully." });
        });
    }        

    public static Guid? GetUserId(HttpContext context) {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                          ?? context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId)) {
            return null;
        }
        return userId;
    }
}


public static class UserContentRoutes {
    public static void RegisterUserContentRoutes(this IEndpointRouteBuilder app) {
        var group = app.MapGroup("/content").RequireAuthorization();

        group.MapGet("/", async (IUserContentService contentService, HttpContext context) => {
            var userId = UserProfileRoutes.GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var content = await contentService.GetAllUserContentAsync(userId.Value);
            return Results.Ok(content);
        });

        group.MapGet("/{contentName}", async (string contentName, IUserContentService contentService, HttpContext context) => {
            var userId = UserProfileRoutes.GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var content = await contentService.GetContentAsync(userId.Value, contentName);
            return content == null ? Results.NotFound() : Results.Ok(content);
        });

        group.MapPost("/", async (CreateContentRequest request, IUserContentService contentService, HttpContext context) => {
            var userId = UserProfileRoutes.GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var success = await contentService.CreateContentAsync(userId.Value, request.ContentName, request.ContentCode);
            return success ? Results.Ok(new { message = "Content created" }) : Results.BadRequest(new { message = "Content with this name already exists" });
        });

        group.MapPut("/{contentName}", async (string contentName, UpdateContentRequest request, IUserContentService contentService, HttpContext context) => {
            var userId = UserProfileRoutes.GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var success = await contentService.UpdateContentAsync(userId.Value, contentName, request.ContentCode);
            return success ? Results.Ok(new { message = "Content updated" }) : Results.NotFound();
        });

        group.MapDelete("/{contentName}", async (string contentName, IUserContentService contentService, HttpContext context) => {
            var userId = UserProfileRoutes.GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var success = await contentService.DeleteContentAsync(userId.Value, contentName);
            return success ? Results.Ok(new { message = "Content deleted" }) : Results.NotFound();
        });
    }        
}
