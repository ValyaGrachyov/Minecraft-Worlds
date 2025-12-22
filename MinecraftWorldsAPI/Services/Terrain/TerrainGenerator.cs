using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Terrain;

/// <summary>
/// Генератор базового ландшафта
/// Заполняет чанк блоками на основе функции плотности: плотность > 0 = камень, <= 0 = воздух
/// </summary>
public class TerrainGenerator : ITerrainGenerator
{
    public void GenerateBaseTerrain(Chunk chunk, IDensityFunction density, IBiomeSource biomeSource)
    {
        var chunkPos = chunk.Position;
        var minX = chunkPos.X * Chunk.SizeX;
        var minZ = chunkPos.Z * Chunk.SizeZ;

        // Проходим по всем блокам в чанке
        for (int x = 0; x < Chunk.SizeX; x++)
        {
            for (int z = 0; z < Chunk.SizeZ; z++)
            {
                var worldX = minX + x;
                var worldZ = minZ + z;

                for (int y = chunk.MinY; y <= chunk.MaxY; y++)
                {
                    // Вычисляем плотность для этого блока
                    var blockDensity = density.ComputeDensity(worldX, y, worldZ);

                    // Плотность > 0 = твердый блок (камень), <= 0 = воздух
                    chunk[x, y, z] = blockDensity > 0 ? Block.Stone : Block.Air;
                }
            }
        }
    }
}
