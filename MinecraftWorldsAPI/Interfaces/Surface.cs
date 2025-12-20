using MinecraftWorldsAPI.Interfaces.biome;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Interfaces;

public interface IFluidFiller
{
    void FillFluids(Chunk chunk, IBiomeSource biomeSource);
}

public interface ISurfaceBuilder
{
    void BuildSurface(Chunk chunk, IBiomeSource biomeSource);
}
