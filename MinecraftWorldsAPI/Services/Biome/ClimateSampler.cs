using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Biome;

public sealed class ClimateSampler(INoiseRegistry noiseRegistry) : IClimateSampler
{
    private const double Scale = 0.01;
    
    public ClimateSample Sample(int x, int y, int z)
    {
        var temperatureNoise = noiseRegistry.GetNoise2D(NoiseNames.Temperature);
        var humidityNoise = noiseRegistry.GetNoise2D(NoiseNames.Humidity);
        
        var temperature = Normalize(
            temperatureNoise.Sample(x * Scale, z * Scale));

        var humidity = Normalize(
            humidityNoise.Sample(x * Scale, z * Scale));

        return new ClimateSample(temperature, humidity);
    }

    private static double Normalize(double v)
        => (v + 1.0) * 0.5; // [-1;1] → [0;1]
}