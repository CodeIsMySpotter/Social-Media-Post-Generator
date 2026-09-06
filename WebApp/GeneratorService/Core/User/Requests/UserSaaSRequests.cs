using GeneratorService.Core.User.Repositories.Models;

namespace GeneratorService.Core.User.Requests;

public class UpdateProfileRequest {
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
}

public class CreateContentRequest {
    public string ContentName { get; set; } = string.Empty;
    public string ContentCode { get; set; } = string.Empty;
}

public class UpdateContentRequest {
    public string ContentCode { get; set; } = string.Empty;
}
