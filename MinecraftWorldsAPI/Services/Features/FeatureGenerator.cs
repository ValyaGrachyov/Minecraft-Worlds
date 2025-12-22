using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Features;

/// <summary>
/// Генерирует объекты (деревья, руды и т.д.) в чанке
/// </summary>
public class FeatureGenerator(IFeatureRegistry? featureRegistry = null) : IFeatureGenerator
{
    public void GenerateFeature(FeatureContext context)
    {
        // Если реестр фич не предоставлен, просто пропускаем генерацию
        if (featureRegistry == null)
            return;
        
        var chunk = context.Chunk;
        var chunkPos = chunk.Position;
        var worldX = chunkPos.X * Chunk.SizeX;
        var worldZ = chunkPos.Z * Chunk.SizeZ;
        
        // Генерируем фичи для случайных позиций в чанке
        var random = context.Random;
        
        // Генерируем несколько случайных позиций для фич
        for (var i = 0; i < 3; i++)
        {
            var x = random.NextInt(0, Chunk.SizeX);
            var z = random.NextInt(0, Chunk.SizeZ);
            var absoluteX = worldX + x;
            var absoluteZ = worldZ + z;
            
            // Находим поверхность
            int? surfaceY = null;
            for (var y = chunk.MaxY; y >= chunk.MinY; y--)
            {
                if (chunk[x, y, z] != Block.Air && chunk[x, y, z] != Block.Water)
                {
                    surfaceY = y;
                    break;
                }
            }
            
            if (!surfaceY.HasValue)
                continue;
            
            var biome = context.BiomeSource.GetBiome(absoluteX, surfaceY.Value, absoluteZ);
            var features = featureRegistry.GetFeaturesForBiome(biome);
            
            foreach (var feature in features)
            {
                feature.Apply(context);
            }
        }
    }
}
