namespace Auth.Features.Sessions.Api.Queries;

public class GetJwks : IFeatureModule
{
    public record JwkEntry(string Kty, string Crv, string X, string Y, string Kid, string Use, string Alg);
    public record Response(JwkEntry[] Keys);

    public static Func<IService, IResult> Handler =>
        (service) =>
        {
            var response = service.Handle();
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/jwks", Handler)
            .AllowAnonymous();
    }

    public interface IService
    {
        Response Handle();
    }

    [Injectable]
    public class Service(IJwtKeyProvider jwtKeyProvider) : IService
    {
        public Response Handle()
        {
            var jwk = jwtKeyProvider.GetJsonWebKey();

            return new Response([
                new JwkEntry(jwk.Kty, jwk.Crv, jwk.X, jwk.Y, jwk.Kid, jwk.Use, jwk.Alg)
            ]);
        }
    }
}
