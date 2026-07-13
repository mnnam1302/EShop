namespace EShop.Shared.RateLimiting;

internal static class RateLimitScriptLoader
{
    public static string Load(string fileName)
    {
        var assembly = typeof(RateLimitScriptLoader).Assembly;
        var resourceName = $"{assembly.GetName().Name}.Scripts.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded Lua script '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
