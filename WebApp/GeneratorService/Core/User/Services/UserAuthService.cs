
using GeneratorService.Core.User.Exceptions;
using GeneratorService.Core.User.Repositories;
using GeneratorService.Core.User.Repositories.Models;
using GeneratorService.Core.User.Requests;
using GeneratorService.Core.User.Services.Subservices;
using BCrypt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GeneratorService.Core.User.Services;

public interface IUserAuthService
{
    Task<string> RegisterAsync(RegisterRequest request);
    Task<string> LoginAsync(LoginRequest request);

}

public class UserAuthService(
    IUserAuthRepository _UserAuthRepository,
    IUserJwtService _userJwtService,
    IUserProfileService _userProfileService
) : IUserAuthService {

    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _UserAuthRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new UserAlreadyExistsException();
        }

        var newUser = new UserAuthModel 
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _UserAuthRepository.CreateAsync(newUser);
        await _userProfileService.CreateProfileAsync(newUser.Id);
        
        return _userJwtService.GetToken(newUser);
    }



    public async Task<string> LoginAsync(LoginRequest request)
    {
        var existingUser = await _UserAuthRepository.GetByEmailAsync(request.Email);
        if (existingUser == null || !BCrypt.Net.BCrypt.Verify(request.Password, existingUser.PasswordHash))
        {
            throw new UserInvalidCredentialsException();
        }

        return _userJwtService.GetToken(existingUser);
    }

}



