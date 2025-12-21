using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Terrain;

/// <summary>
/// Генерирует базовый рельеф чанка (камень и воздух)
/// </summary>
public class TerrainGenerator : ITerrainGenerator
{
    public void GenerateBaseTerrain(Chunk chunk, IDensityFunction density, IBiomeSource biomeSource)
    {
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
                    var densityValue = density.ComputeDensity(absoluteX, y, absoluteZ);
                    
                    // Если плотность положительная, ставим камень, иначе воздух
                    chunk[x, y, z] = densityValue > 0 ? Block.Stone : Block.Air;
                }
            }
        }
    }
}
