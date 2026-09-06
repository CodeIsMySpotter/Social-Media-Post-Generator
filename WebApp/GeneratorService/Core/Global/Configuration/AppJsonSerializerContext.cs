using System.Text.Json.Serialization;
using GeneratorService.Core.User.Repositories.Models;
using GeneratorService.Core.User.Requests;

namespace GeneratorService.Core.Global.Configuration;

[JsonSerializable(typeof(RegisterRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(UpdateProfileRequest))]
[JsonSerializable(typeof(CreateContentRequest))]
[JsonSerializable(typeof(UpdateContentRequest))]
[JsonSerializable(typeof(UserProfileModel))]
[JsonSerializable(typeof(UserContentModel))]
[JsonSerializable(typeof(List<UserContentModel>))]
[JsonSerializable(typeof(object))] // For anonymous objects used in Results.Ok(new { message = "..." })
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
