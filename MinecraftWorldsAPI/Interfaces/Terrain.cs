using MinecraftWorldsAPI.Interfaces.biome;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Interfaces;

public interface IDensityFunction
{
    double ComputeDensity(int x, int y, int z);
}

public interface ITerrainGenerator
{
    /// <summary>
    /// Generates only terrain: air or solid blocks (stone)
    /// </summary>
    void GenerateBaseTerrain(Chunk chunk, IDensityFunction density, IBiomeSource biomeSource);
}
