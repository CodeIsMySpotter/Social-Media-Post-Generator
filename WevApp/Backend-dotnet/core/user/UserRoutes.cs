using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using BackendDotnet.Core.User.Services;
using BackendDotnet.Core.User.Requests;

namespace BackendDotnet.Core.User;

public static class UserAuthRoutes {
    public static void RegisterUserAuthRoutes(this WebApplication app) {
        app.MapPost("/register", (RegisterRequest request, IUserAuthService service) => {
            return service.RegisterAsync(request);
        });

        app.MapPost("/login", (LoginRequest request, IUserAuthService service) => {
            return service.LoginAsync(request);
        });

        app.MapPost("/logout", (IUserAuthService service) => {
            return service.LogoutAsync();
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