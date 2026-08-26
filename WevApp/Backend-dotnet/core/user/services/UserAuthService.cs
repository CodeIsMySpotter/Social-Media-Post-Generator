namespace BackendDotnet.Core.User.Services;

public interface IUserAuthService
{
    Task<string> RegisterAsync();
    Task<string> LoginAsync();
    Task<bool> LogoutAsync();
}

public class UserAuthService : IUserAuthService
{
    //private readonly IUserRepository _userRepository;
    
    public UserAuthService()
    {
        //_userRepository = userRepository;
    }

    public async Task<string> RegisterAsync()
    {
        return await Task.FromResult("Registered");
    }

    public async Task<string> LoginAsync()
    {
        return await Task.FromResult("Logged in");
    }

    public async Task<bool> LogoutAsync()
    {
        return await Task.FromResult(true);
    }
}
