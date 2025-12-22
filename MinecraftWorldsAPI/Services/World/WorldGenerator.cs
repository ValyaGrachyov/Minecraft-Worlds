using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Features;

namespace MinecraftWorldsAPI.Services.World;

/// <summary>
/// Основной генератор мира, координирует все этапы генерации чанка
/// </summary>
public class WorldGenerator(
    IRandomFactory randomFactory,
    IDensityFunction densityFunction,
    ITerrainGenerator terrainGenerator,
    ICaveGenerator caveGenerator,
    IFluidFiller fluidFiller,
    ISurfaceBuilder surfaceBuilder,
    INoiseRegistry noiseRegistry,
    IBiomeSource biomeSource)
    : IWorldGenerator
{
    public Chunk GenerateChunk(ChunkPos chunkPos, long seed)
    {
        var featureRegistry = new FeatureRegistry();
        var featureGenerator = new FeatureGenerator(featureRegistry);

        // Создаем генератор случайных чисел для чанка
        var random = randomFactory.CreateForChunk(seed, chunkPos, 0);

        // Генерируем чанк
        var chunk = new Chunk(chunkPos, 0, 255);

        // Этапы генерации чанка
        terrainGenerator.GenerateBaseTerrain(chunk, densityFunction, biomeSource);

        caveGenerator.Carve(chunk, biomeSource, noiseRegistry);

        fluidFiller.FillFluids(chunk, biomeSource);

        surfaceBuilder.BuildSurface(chunk, biomeSource);

        featureGenerator.GenerateFeature(new FeatureContext(chunk, biomeSource, random.Fork(1)));

        return chunk;
    }
}
