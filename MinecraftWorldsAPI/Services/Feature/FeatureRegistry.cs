using MinecraftWorldsAPI.Interfaces;

namespace MinecraftWorldsAPI.Services.Feature;

public sealed class FeatureRegistry : IFeatureRegistry
{
    private readonly Dictionary<Models.Biome, List<IFeature>> _features = new()
    {
        { Models.Biome.Forest, [new TreeFeature()] },
        { Models.Biome.Plains, [new TreeFeature()] }
    };

    public IReadOnlyList<IFeature> GetFeaturesForBiome(Models.Biome biome) =>
        _features.TryGetValue(biome, out var list)
            ? list
            : Array.Empty<IFeature>();
}
