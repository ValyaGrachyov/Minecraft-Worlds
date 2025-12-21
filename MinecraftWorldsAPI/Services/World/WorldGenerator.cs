using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Biome;
using MinecraftWorldsAPI.Services.Caves;
using MinecraftWorldsAPI.Services.Features;
using MinecraftWorldsAPI.Services.Fluids;
using MinecraftWorldsAPI.Services.Noise;
using MinecraftWorldsAPI.Services.Surface;
using MinecraftWorldsAPI.Services.Terrain;

namespace MinecraftWorldsAPI.Services.World;

/// <summary>
/// Основной генератор мира, координирует все этапы генерации чанка
/// </summary>
public class WorldGenerator(IRandomFactory randomFactory) : IWorldGenerator
{
    public Chunk GenerateChunk(ChunkPos chunkPos, long seed)
    {
        // Создаем NoiseRegistry для данного seed
        var noiseRegistryFactory = new NoiseRegistryFactory(randomFactory);
        var noiseRegistry = noiseRegistryFactory.Create(seed);
        
        // Получаем BiomeSource из DI (он уже зарегистрирован)
        // Но для простоты создаем здесь, так как он зависит от ClimateSampler
        // В реальности лучше использовать фабрику или передавать через параметры
        
        // Создаем остальные сервисы
        var densityFunction = new DensityFunction(noiseRegistry);
        var terrainGenerator = new TerrainGenerator();
        var caveGenerator = new CaveGenerator();
        var fluidFiller = new FluidFiller();
        var surfaceBuilder = new SurfaceBuilder();
        var featureRegistry = new FeatureRegistry();
        var featureGenerator = new FeatureGenerator(featureRegistry);
        
        // Создаем генератор случайных чисел для чанка
        var random = randomFactory.CreateForChunk(seed, chunkPos, 0);
        
        // Генерируем чанк
        var chunk = new Chunk(chunkPos, -64, 320);
        
        // Для BiomeSource нужно создать ClimateSampler
        // Используем те же шумы, что и для климата, но с другими seed
        var temperatureNoise = new PerlinNoise2D(
            randomFactory,
            seed + 1001,
            frequency: 0.01,
            amplitude: 1.0,
            octaves: 4,
            lacunarity: 2.0,
            persistence: 0.5
        );
        
        var humidityNoise = new PerlinNoise2D(
            randomFactory,
            seed + 2002,
            frequency: 0.01,
            amplitude: 1.0,
            octaves: 4,
            lacunarity: 2.0,
            persistence: 0.5
        );
        
        var climateSampler = new ClimateSampler(temperatureNoise, humidityNoise);
        var biomeSource = new BiomeSource(climateSampler);
        
        // Этапы генерации чанка
        terrainGenerator.GenerateBaseTerrain(chunk, densityFunction, biomeSource);
        
        caveGenerator.Carve(chunk, biomeSource, noiseRegistry);
        
        fluidFiller.FillFluids(chunk, biomeSource);
        
        surfaceBuilder.BuildSurface(chunk, biomeSource);
        
        featureGenerator.GenerateFeature(new FeatureContext(chunk, biomeSource, random.Fork(1)));
        
        return chunk;
    }
}
