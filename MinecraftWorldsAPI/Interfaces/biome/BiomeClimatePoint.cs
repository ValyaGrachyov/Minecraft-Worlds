using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Interfaces.biome;

public readonly struct BiomeClimatePoint(Biome biome, double temperature, double humidity)
{
    public Biome Biome { get; } = biome;
    public double Temperature { get; } = temperature;
    public double Humidity { get; } = humidity;
}
