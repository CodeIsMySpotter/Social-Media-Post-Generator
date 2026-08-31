using GeneratorService.Core.User.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorService.Core.User.ExceptionHandlers;

public class UserAuthExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UserNotFoundException and 
            not UserAlreadyExistsException and 
            not UserInvalidCredentialsException and
            not UserAuthInternalException)
        {
            return false;
        }

        var (statusCode, title) = exception switch
        {
            UserNotFoundException => (StatusCodes.Status404NotFound, "User Not Found"),
            UserAlreadyExistsException => (StatusCodes.Status409Conflict, "User Already Exists"),
            UserInvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            UserAuthInternalException => (StatusCodes.Status500InternalServerError, "User Auth Internal Error"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
