using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Interfaces;

public interface IClimateSampler
{
    ClimateSample Sample(int x, int y, int z);
}

public interface IBiomeSource
{
    Biome GetBiome(int x, int y, int z);
}