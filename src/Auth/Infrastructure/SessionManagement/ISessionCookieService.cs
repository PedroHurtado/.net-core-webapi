namespace Auth.Infrastructure.SessionManagement;

public interface ISessionCookieService
{
    void Append(HttpContext httpContext, Guid sessionId);
}
