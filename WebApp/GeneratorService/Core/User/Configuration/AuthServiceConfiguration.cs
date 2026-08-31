

namespace GeneratorService.Core.User.Configuration;
    
public class AuthServiceConfiguration {
    public required string Secret { get; set; }
    public required int ExpiryDays { get; set; }
    public required string Issuer { get; set; }
    public required string Audience {get; set; }
}
