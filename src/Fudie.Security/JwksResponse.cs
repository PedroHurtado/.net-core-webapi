namespace Fudie.Security;

public record JwkEntry(string Kty, string Crv, string X, string Y, string Kid, string Use, string Alg);

public record JwksResponse(JwkEntry[] Keys);
