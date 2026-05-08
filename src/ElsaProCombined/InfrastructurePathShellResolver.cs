using CShells;
using CShells.Resolution;
using Microsoft.Extensions.Configuration;

namespace ElsaProCombined;

/// <summary>
/// Prevents application infrastructure requests from entering a CShells shell scope.
/// </summary>
/// <remarks>
/// The combined application serves both shell-scoped Elsa API/runtime endpoints and host-level Blazor Studio assets.
/// Blazor Server infrastructure such as <c>/_blazor</c> must stay on the root application service provider so that
/// SignalR circuit state and prerendered component records are created from the same provider.
/// </remarks>
internal sealed class InfrastructurePathShellResolver : IShellResolver
{
    private static readonly string[] ExcludedPathPrefixes =
    [
        "/_blazor",
        "/_framework",
        "/_content",
        "/app.css",
        "/appsettings.json",
        "/favicon.ico",
        "/ElsaProCombined.styles.css",
        "/ElsaProCombined.modules.json"
    ];

    private readonly DefaultShellResolver innerResolver;
    private readonly string[] shellPathPrefixes;

    /// <summary>
    /// Initializes a new instance of the <see cref="InfrastructurePathShellResolver"/> class.
    /// </summary>
    /// <param name="strategies">The shell resolver strategies to delegate to for shell-owned paths.</param>
    /// <param name="options">The resolver ordering options used by the delegated resolver.</param>
    /// <param name="configuration">The application configuration used to discover shell-owned path prefixes.</param>
    public InfrastructurePathShellResolver(IEnumerable<IShellResolverStrategy> strategies, ShellResolverOptions options, IConfiguration configuration)
    {
        innerResolver = new(strategies, options);
        shellPathPrefixes = GetShellPathPrefixes(configuration).ToArray();
    }

    /// <inheritdoc />
    public Task<ShellId?> ResolveAsync(ShellResolutionContext context, CancellationToken cancellationToken = default)
    {
        var path = context.Get<string>("Path");

        if (ExcludedPathPrefixes.Any(prefix => path?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true))
            return Task.FromResult<ShellId?>(null);

        if (IsHtmlNavigationRequest(context) && !IsShellPath(path))
            return Task.FromResult<ShellId?>(null);

        return innerResolver.ResolveAsync(context, cancellationToken);
    }

    private bool IsShellPath(string? path)
    {
        return shellPathPrefixes.Any(prefix => path?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true);
    }

    private static bool IsHtmlNavigationRequest(ShellResolutionContext context)
    {
        var headers = context.Get<Dictionary<string, string>>("Headers");

        return headers?.TryGetValue("Accept", out var accept) == true &&
               accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> GetShellPathPrefixes(IConfiguration configuration)
    {
        yield return GetPath(configuration.GetValue<string>("Backend:Url") ?? "/elsa/api");
        yield return "/elsa/api";
        yield return "/workflows";

        foreach (var shellSection in configuration.GetSection("CShells:Shells").GetChildren())
        {
            var basePath = shellSection.GetValue<string>("Features:Http:HttpActivityOptions:BasePath");

            if (!string.IsNullOrWhiteSpace(basePath))
                yield return GetPath(basePath);
        }
    }

    private static string GetPath(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri.AbsolutePath : value;
    }
}
