namespace Auth.Infrastructure.Jwt;

public class DevJwtKeyProvider(IConfiguration configuration) : IJwtKeyProvider
{
    private readonly Lazy<ECDsa> _ecdsa = new(() =>
    {
        var base64Key = configuration["Jwt:PrivateKey"]
            ?? throw new InvalidOperationException("Jwt:PrivateKey secret not found");

        var keyBytes = Convert.FromBase64String(base64Key);
        var ecdsa = ECDsa.Create();
        ecdsa.ImportECPrivateKey(keyBytes, out _);
        return ecdsa;
    });

    private readonly string _kid = configuration["Jwt:Kid"]
        ?? throw new InvalidOperationException("Jwt:Kid secret not found");

    public ECDsa GetPrivateKey() => _ecdsa.Value;

    public JsonWebKey GetJsonWebKey()
    {
        var parameters = _ecdsa.Value.ExportParameters(includePrivateParameters: false);

        return new JsonWebKey
        {
            Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
            Crv = "P-256",
            X = Base64UrlEncoder.Encode(parameters.Q.X!),
            Y = Base64UrlEncoder.Encode(parameters.Q.Y!),
            Kid = _kid,
            Use = "sig",
            Alg = SecurityAlgorithms.EcdsaSha256
        };
    }
}