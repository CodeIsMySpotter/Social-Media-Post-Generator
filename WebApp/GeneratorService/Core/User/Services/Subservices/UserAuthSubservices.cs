using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GeneratorService.Core.User.Configuration;
using GeneratorService.Core.User.Repositories.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GeneratorService.Core.User.Services.Subservices;



public interface IUserJwtService {
    public string GetToken(UserAuthModel UserAuthModel);
}


public class UserJwtService(IOptions<AuthServiceConfiguration> _AuthServiceOptions) : IUserJwtService {


    public string GetToken(UserAuthModel UserAuthModel) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_AuthServiceOptions.Value.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(new[] {
                new Claim(JwtRegisteredClaimNames.Sub, UserAuthModel.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, UserAuthModel.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(_AuthServiceOptions.Value.ExpiryDays),
            Issuer = _AuthServiceOptions.Value.Issuer,
            Audience = _AuthServiceOptions.Value.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

