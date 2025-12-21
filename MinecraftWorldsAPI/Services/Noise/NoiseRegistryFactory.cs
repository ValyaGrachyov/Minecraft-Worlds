using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Noise;

/// <summary>
/// Фабрика для создания NoiseRegistry с предустановленными шумами
/// </summary>
public class NoiseRegistryFactory(IRandomFactory randomFactory)
{
    public INoiseRegistry Create(long seed)
    {
        var registry = new NoiseRegistry(randomFactory, seed);
        
        // Регистрируем основные шумы для генерации мира
        registry.RegisterNoise2D("terrain_height", new NoiseSettings(
            Frequency: 0.01,
            Amplitude: 1.0,
            Octaves: 4,
            Lacunarity: 2.0,
            Persistence: 0.5
        ));
        
        registry.RegisterNoise("terrain_detail", new NoiseSettings(
            Frequency: 0.02,
            Amplitude: 1.0,
            Octaves: 3,
            Lacunarity: 2.0,
            Persistence: 0.5
        ));
        
        registry.RegisterNoise("caves", new NoiseSettings(
            Frequency: 0.05,
            Amplitude: 1.0,
            Octaves: 2,
            Lacunarity: 2.0,
            Persistence: 0.5
        ));
        
        return registry;
    }
}
