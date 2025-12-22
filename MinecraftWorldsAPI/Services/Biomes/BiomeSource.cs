using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Biomes;


public sealed class BiomeSource(IClimateSampler climateSampler) : IBiomeSource
{
    private static readonly BiomeClimatePoint[] Biomes =
    [
        new(Biome.Desert,     0.9, 0.1),
        new(Biome.Plains,     0.5, 0.4),
        new(Biome.Forest,     0.6, 0.8),
        new(Biome.Mountains, 0.2, 0.3),
        new(Biome.Ocean,      0.5, 0.5)
    ];

    public Biome GetBiome(int x, int y, int z)
    {
        var climate = climateSampler.Sample(x, y, z);

        BiomeClimatePoint best = default;
        var bestDist = double.MaxValue;

        foreach (var biome in Biomes)
        {
            var d =
                Sq(climate.Temperature - biome.Temperature) +
                Sq(climate.Humidity - biome.Humidity);

            if (!(d < bestDist)) continue;
            
            bestDist = d;
            best = biome;
        }

        return best.Biome;
    }

    private static double Sq(double v) => v * v;
}
