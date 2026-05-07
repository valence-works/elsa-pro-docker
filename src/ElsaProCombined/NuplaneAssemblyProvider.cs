using System.Reflection;
using CShells.Features;
using Nuplane.Loading;

namespace ElsaProCombined;

internal sealed class NuplaneAssemblyProvider(IPackageAssemblyCatalog packageLoader) : IFeatureAssemblyProvider
{
    public async Task<IEnumerable<Assembly>> GetAssembliesAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return await packageLoader.GetAssembliesAsync(cancellationToken);
    }
}
