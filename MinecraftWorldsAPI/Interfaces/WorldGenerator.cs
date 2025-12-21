using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Interfaces;

public interface IWorldGenerator
{
    Chunk GenerateChunk(ChunkPos chunkPos, long seed);
}

public class WorldGeneratorExample(
    // PRNG
    IRandomFactory randomFactory,
    // Noise
    INoiseRegistry noiseRegistry,
    // Biome
    IBiomeSource biomeSource,
    // Terrain + Surface
    IDensityFunction densityFunction,
    ITerrainGenerator terrainGenerator,
    ICaveGenerator caveGenerator,
    IFluidFiller fluidFiller,
    ISurfaceBuilder surfaceBuilder,
    // Features
    IFeatureGenerator featureGenerator
) : IWorldGenerator
{
    public Chunk GenerateChunk(ChunkPos chunkPos, long seed)
    {
        var random = randomFactory.CreateForChunk(seed, chunkPos, 0);

        var chunk = new Chunk(chunkPos, -64, 320);

        terrainGenerator.GenerateBaseTerrain(chunk, densityFunction, biomeSource);

        caveGenerator.Carve(chunk, biomeSource, noiseRegistry);

        fluidFiller.FillFluids(chunk, biomeSource);

        surfaceBuilder.BuildSurface(chunk, biomeSource);

        featureGenerator.GenerateFeature(new FeatureContext(chunk, biomeSource, random.Fork(1)));

        return chunk;
    }
}