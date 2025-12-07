using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Interfaces;

public interface IFeature
{
    void Apply(FeatureContext context);
}

public interface IFeatureRegistry
{
    IReadOnlyList<IFeature> GetFeaturesForBiome(Biome biome);
}

public interface IFeatureGenerator
{
    void GenerateFeature(FeatureContext context);
}
