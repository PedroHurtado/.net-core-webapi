namespace Fudie.Security;

public class FudieSecurityOptions
{
    public const string SectionName = "Fudie:Security";

    public string JwksUrl { get; set; } = string.Empty;
    public int CacheRefreshMinutes { get; set; } = 60;
}
