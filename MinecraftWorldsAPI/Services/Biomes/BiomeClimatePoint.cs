using MinecraftWorldsAPI.Models;
namespace MinecraftWorldsAPI.Services.Biomes;

public readonly record struct BiomeClimatePoint(Biome biome, double temperature, double humidity)
{
    public Biome Biome { get; } = biome;
    public double Temperature { get; } = temperature;
    public double Humidity { get; } = humidity;
}
