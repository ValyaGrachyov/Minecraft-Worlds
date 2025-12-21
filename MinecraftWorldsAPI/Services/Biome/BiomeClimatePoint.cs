namespace MinecraftWorldsAPI.Services.Biome;

public readonly struct BiomeClimatePoint(Models.Biome biome, double temperature, double humidity)
{
    public Models.Biome Biome { get; } = biome;
    public double Temperature { get; } = temperature;
    public double Humidity { get; } = humidity;
}
