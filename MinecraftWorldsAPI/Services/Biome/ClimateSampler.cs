using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Biome;

public sealed class ClimateSampler(
    INoise2D temperatureNoise,
    INoise2D humidityNoise) : IClimateSampler
{
    
    private const double Scale = 0.01;
    
    public ClimateSample Sample(int x, int y, int z)
    {
        var temperature = Normalize(
            temperatureNoise.Sample(x * Scale, z * Scale));

        var humidity = Normalize(
            humidityNoise.Sample(x * Scale, z * Scale));

        return new ClimateSample(temperature, humidity);
    }

    private static double Normalize(double v)
        => (v + 1.0) * 0.5; // [-1;1] → [0;1]
}