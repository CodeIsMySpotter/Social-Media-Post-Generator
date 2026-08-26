namespace BackendDotnet.Core.User.Requests;



public record RegisterRequest (string Email, string Password);
public record LoginRequest(string Email, string Password);
