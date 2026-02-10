using System.Diagnostics;
using System.Security.Cryptography;

Console.WriteLine("Generating ES256 (P-256) key pair for JWT signing...\n");

var solutionRoot = FindSolutionRoot()
    ?? throw new InvalidOperationException("Could not find solution root (webapi.sln)");

var authCsproj = Path.Combine(solutionRoot, "src", "Auth", "Auth.csproj");

if (!File.Exists(authCsproj))
    throw new FileNotFoundException($"Auth.csproj not found at: {authCsproj}");

using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var base64PrivateKey = Convert.ToBase64String(ecdsa.ExportECPrivateKey());
var kid = "auth-dev-001";

SetSecret(authCsproj, "Jwt:PrivateKey", base64PrivateKey);
SetSecret(authCsproj, "Jwt:Kid", kid);

Console.WriteLine($"\nKey ID: {kid}");
Console.WriteLine("ES256 key pair stored in Auth user secrets.");

var googleOauthPath = Path.Combine(solutionRoot, "src", "Auth", "google-oauth.json");

if (File.Exists(googleOauthPath))
{
    Console.WriteLine("\nSetting Google OAuth credentials from google-oauth.json...\n");
    var json = File.ReadAllText(googleOauthPath).ReplaceLineEndings("").Replace("  ", "");
    SetSecret(authCsproj, "Google:Oauth", json);
    Console.WriteLine("\nGoogle OAuth credentials stored in Auth user secrets.");
}
else
{
    Console.WriteLine($"\nWarning: google-oauth.json not found at {googleOauthPath}");
    Console.WriteLine("Run 'git-crypt unlock <key-file>' to decrypt OAuth secrets.");
}

static string? FindSolutionRoot()
{
    var dir = AppContext.BaseDirectory;

    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir, "webapi.sln")))
            return dir;

        dir = Directory.GetParent(dir)?.FullName;
    }

    return null;
}

static void SetSecret(string projectPath, string key, string value)
{
    var psi = new ProcessStartInfo
    {
        FileName = "dotnet",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    psi.ArgumentList.Add("user-secrets");
    psi.ArgumentList.Add("set");
    psi.ArgumentList.Add(key);
    psi.ArgumentList.Add(value);
    psi.ArgumentList.Add("--project");
    psi.ArgumentList.Add(projectPath);

    using var process = Process.Start(psi)!;
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        var error = process.StandardError.ReadToEnd();
        Console.Error.WriteLine($"Failed to set '{key}': {error}");
        Environment.Exit(1);
    }

    Console.WriteLine($"  Set {key}");
}