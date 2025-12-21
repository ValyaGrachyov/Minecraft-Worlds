using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Caves;

/// <summary>
/// Генерирует пещеры в чанке, удаляя блоки на основе шума
/// </summary>
public class CaveGenerator : ICaveGenerator
{
    private const double CaveScale = 0.05;
    private const double CaveThreshold = 0.3;
    
    public void Carve(Chunk chunk, IBiomeSource biomeSource, INoiseRegistry noiseRegistry)
    {
        var caveNoise = noiseRegistry.GetNoise("caves");
        var chunkPos = chunk.Position;
        var worldX = chunkPos.X * Chunk.SizeX;
        var worldZ = chunkPos.Z * Chunk.SizeZ;
        
        for (var x = 0; x < Chunk.SizeX; x++)
        {
            for (var z = 0; z < Chunk.SizeZ; z++)
            {
                var absoluteX = worldX + x;
                var absoluteZ = worldZ + z;
                
                for (var y = chunk.MinY; y <= chunk.MaxY; y++)
                {
                    // Пропускаем воздух
                    if (chunk[x, y, z] == Block.Air)
                        continue;
                    
                    // Вычисляем значение шума пещер
                    var caveValue = caveNoise.Sample(
                        absoluteX * CaveScale,
                        y * CaveScale,
                        absoluteZ * CaveScale
                    );
                    
                    // Если значение шума выше порога, создаем пещеру (удаляем блок)
                    if (caveValue > CaveThreshold)
                    {
                        chunk[x, y, z] = Block.Air;
                    }
                }
            }
        }
    }
}
