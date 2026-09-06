using GeneratorService.Core.Generator.Services;
using GeneratorService.Core.User;
using GeneratorService.Core.User.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GeneratorService.Core.Generator;

public static class GeneratorRoutes {
    public static void RegisterGeneratorRoutes(this IEndpointRouteBuilder app) {
        var group = app.MapGroup("/generate").RequireAuthorization();

        group.MapPost("/image/{contentName}", async (string contentName, IUserContentService contentService, IUserProfileService profileService, IMediaGeneratorService mediaGenerator, HttpContext context) => {
            var userId = UserProfileRoutes.GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            // 1. Check credits (cost 1 credit)
            var hasCredits = await profileService.HasEnoughCreditsAsync(userId.Value, 1);
            if (!hasCredits) return Results.BadRequest(new { message = "Not enough credits." });

            // 2. Verify content exists
            var content = await contentService.GetContentAsync(userId.Value, contentName);
            if (content == null) return Results.NotFound(new { message = "Content not found." });

            // 3. Generate Image
            try {
                // In a real app, this would be the frontend URL. For local dev with Docker, it might be http://frontend:5173/render?contentId=...
                string frontendUrl = $"http://localhost:5173/render?contentId={content.Id}"; 
                var imageBytes = await mediaGenerator.GenerateImageAsync(frontendUrl);
                
                // Deduct credits on success
                await profileService.DeductCreditsAsync(userId.Value, 1);

                return Results.File(imageBytes, "image/png", $"{contentName}.png");
            }
            catch (Exception ex) {
                return Results.Problem($"Failed to generate image: {ex.Message}");
            }
        });

        group.MapPost("/video/{contentName}", async (string contentName, IUserContentService contentService, IUserProfileService profileService, IMediaGeneratorService mediaGenerator, HttpContext context) => {
            var userId = UserProfileRoutes.GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            // 1. Check credits (cost 5 credits for video)
            var hasCredits = await profileService.HasEnoughCreditsAsync(userId.Value, 5);
            if (!hasCredits) return Results.BadRequest(new { message = "Not enough credits." });

            // 2. Verify content exists
            var content = await contentService.GetContentAsync(userId.Value, contentName);
            if (content == null) return Results.NotFound(new { message = "Content not found." });

            // 3. Generate Video
            try {
                string frontendUrl = $"http://localhost:5173/render?contentId={content.Id}";
                var videoBytes = await mediaGenerator.GenerateVideoAsync(frontendUrl, fps: 60, durationSeconds: 5);
                
                await profileService.DeductCreditsAsync(userId.Value, 5);

                return Results.File(videoBytes, "video/mp4", $"{contentName}.mp4");
            }
            catch (Exception ex) {
                return Results.Problem($"Failed to generate video: {ex.Message}");
            }
        });
    }
}
