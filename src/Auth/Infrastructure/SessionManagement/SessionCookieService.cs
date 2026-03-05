namespace Auth.Infrastructure.SessionManagement;

[Injectable]
public class SessionCookieService(
    IRequestTimestamp requestTimestamp,
    ISessionSettings sessionSettings) : ISessionCookieService
{
    public void Append(HttpContext httpContext, Guid sessionId)
    {
        var settings = sessionSettings.Get();

        httpContext.Response.Cookies.Append(settings.CookieName, sessionId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = new DateTimeOffset(requestTimestamp.ExpiresAt, TimeSpan.Zero)
        });
    }

    public void Delete(HttpContext httpContext)
    {
        var settings = sessionSettings.Get();
        httpContext.Response.Cookies.Delete(settings.CookieName);
    }
}
