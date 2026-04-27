namespace CNS.Infrastructure.Hosting;

public static class AppEnvironmentNameResolver
{
    public const string Development = "Development";
    public const string Staging = "Staging";
    public const string Production = "Production";

    /// <summary>
    /// Reads <c>DOTNET_ENVIRONMENT</c> first, then <c>ASPNETCORE_ENVIRONMENT</c>.
    /// Accepts common aliases (dev/stage/prod). Unknown values fall back to Development.
    /// </summary>
    public static string Resolve()
    {
        var raw =
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(raw))
            return Development;

        raw = raw.Trim();

        if (IsDevelopment(raw))
            return Development;
        if (IsStaging(raw))
            return Staging;
        if (IsProduction(raw))
            return Production;

        return Development;
    }

    private static bool IsDevelopment(string value) =>
        string.Equals(value, Development, StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "dev", StringComparison.OrdinalIgnoreCase);

    private static bool IsStaging(string value) =>
        string.Equals(value, Staging, StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "stage", StringComparison.OrdinalIgnoreCase);

    private static bool IsProduction(string value) =>
        string.Equals(value, Production, StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "prod", StringComparison.OrdinalIgnoreCase);
}
