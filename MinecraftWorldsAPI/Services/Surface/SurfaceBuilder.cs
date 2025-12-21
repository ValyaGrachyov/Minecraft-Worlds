using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Surface;

/// <summary>
/// Строит поверхность чанка, заменяя камень на соответствующие блоки в зависимости от биома
/// </summary>
public class SurfaceBuilder : ISurfaceBuilder
{
    public void BuildSurface(Chunk chunk, IBiomeSource biomeSource)
    {
        var chunkPos = chunk.Position;
        var worldX = chunkPos.X * Chunk.SizeX;
        var worldZ = chunkPos.Z * Chunk.SizeZ;
        
        for (var k = 0; k < Chunk.SizeX; k++)
        {
            for (var l = 0; l < Chunk.SizeZ; l++)
            {
                var absoluteX = worldX + k;
                var absoluteZ = worldZ + l;
                
                // Находим поверхность (первый не-воздушный блок сверху)
                int? surfaceY = null;
                for (var i = chunk.MaxY; i >= chunk.MinY; i--)
                {
                    if (chunk[k, i, l] != Block.Air && chunk[k, i, l] != Block.Water)
                    {
                        surfaceY = i;
                        break;
                    }
                }
                
                if (!surfaceY.HasValue)
                    continue;
                
                var y = surfaceY.Value;
                var biome = biomeSource.GetBiome(absoluteX, y, absoluteZ);
                
                // Определяем блоки поверхности в зависимости от биома
                switch (biome)
                {
                    case Biome.Desert:
                        chunk[k, y,l] = Block.Dirt; // Песок можно добавить позже
                        break;
                    
                    case Biome.Plains:
                    case Biome.Forest:
                        // Трава на поверхности, земля под ней
                        if (y < chunk.MaxY && chunk[k, y + 1, l] == Block.Air)
                        {
                            chunk[k, y, l] = Block.Grass;
                            
                            // Добавляем слой земли под травой
                            if (y > chunk.MinY && chunk[k, y - 1, l] == Block.Stone)
                            {
                                chunk[k, y - 1, l] = Block.Dirt;
                            }
                        }
                        break;
                    
                    case Biome.Mountains:
                        chunk[k, y, l] = Block.Stone;
                        break;
                    
                    case Biome.Ocean:
                        // В океане поверхность уже обработана водой
                        break;
                    
                    default:
                        chunk[k, y, l] = Block.Grass;
                        break;
                }
            }
        }
    }
}
