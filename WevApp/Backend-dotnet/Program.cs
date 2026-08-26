using BackendDotnet.Core.User;
using BackendDotnet.Core.User.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserAuthService, UserAuthService>();


builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.RegisterUserAuthRoutes();
app.Run();


