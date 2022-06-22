namespace ReactSpa_Backend.Authorization;

using Microsoft.Extensions.Options;
using ReactSpa_Backend.Helpers;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppSettings _appSettings;

    public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
    {
        _next = next;
        _appSettings = appSettings.Value;
    }

    public async Task Invoke(HttpContext context, DataContext dataContext, IJwtUtils jwtUtils)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var userId = jwtUtils.ValidateToken(token);

        if (userId != null)
        {
            context.Items["user"] = await dataContext.Users.FindAsync(userId.Value);
        }

        await _next(context);
    }
}