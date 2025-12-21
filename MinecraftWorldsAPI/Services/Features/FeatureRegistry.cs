using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Features;

/// <summary>
/// Реестр объектов для генерации в зависимости от биома
/// </summary>
public class FeatureRegistry : IFeatureRegistry
{
    private readonly Dictionary<Models.Biome, List<IFeature>> _featuresByBiome = new();
    
    
    public void RegisterFeature(Models.Biome biome, IFeature feature)
    {
        if (!_featuresByBiome.ContainsKey(biome))
        {
            _featuresByBiome[biome] = new List<IFeature>();
        }
        
        _featuresByBiome[biome].Add(feature);
    }

    public IReadOnlyList<IFeature> GetFeaturesForBiome(Models.Biome biome)
    {
        return _featuresByBiome.TryGetValue(biome, out var features)
            ? features.AsReadOnly()
            : Array.Empty<IFeature>();
    }
}
