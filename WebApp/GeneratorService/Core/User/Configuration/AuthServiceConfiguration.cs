

namespace GeneratorService.Core.User.Configuration;
    
public class AuthServiceConfiguration {
    public string Secret { get; set; } = string.Empty;
    public int ExpiryDays { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string Audience {get; set; } = string.Empty;
}
