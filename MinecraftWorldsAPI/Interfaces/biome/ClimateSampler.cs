using MinecraftWorldsAPI.Interfaces.noise;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Interfaces.biome;

public interface IClimateSampler
{
    ClimateSample Sample(int x, int y, int z);
}

public sealed class ClimateSampler(
    INoise2D temperatureNoise,
    INoise2D humidityNoise) : IClimateSampler
{
    public ClimateSample Sample(int x, int y, int z)
    {
        var temperature = Normalize(temperatureNoise.Sample(x, z));
        var humidity = Normalize(humidityNoise.Sample(x, z));

        return new ClimateSample(temperature, humidity);
    }

    private static double Normalize(double v)
        => (v + 1.0) * 0.5; // [-1;1] → [0;1]
}
