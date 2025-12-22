using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Feature;

public sealed class FeatureGenerator(IFeatureRegistry registry) : IFeatureGenerator
{
    public void GenerateFeature(FeatureContext context)
    {
        // берём биом центра чанка
        var biome = context.BiomeSource.GetBiome(
            context.Chunk.Position.X * Chunk.SizeX,
            0,
            context.Chunk.Position.Z * Chunk.SizeZ
        );

        foreach (var feature in registry.GetFeaturesForBiome(biome))
        {
            feature.Apply(context);
        }
    }
}
