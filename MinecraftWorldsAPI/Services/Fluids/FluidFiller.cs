using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Fluids;

/// <summary>
/// Заполняет чанк жидкостями (водой) в зависимости от биома
/// </summary>
public class FluidFiller : IFluidFiller
{
    private const int SeaLevel = 63;
    
    public void FillFluids(Chunk chunk, IBiomeSource biomeSource)
    {
        for (var x = 0; x < Chunk.SizeX; x++)
        {
            for (var z = 0; z < Chunk.SizeZ; z++)
            {
                var chunkPos = chunk.Position;
                var worldX = chunkPos.X * Chunk.SizeX + x;
                var worldZ = chunkPos.Z * Chunk.SizeZ + z;
                
                var biome = biomeSource.GetBiome(worldX, SeaLevel, worldZ);
                
                // Заполняем водой только в океанах и ниже уровня моря
                if (biome == Biome.Ocean)
                {
                    for (var y = chunk.MinY; y <= SeaLevel; y++)
                    {
                        if (chunk[x, y, z] == Block.Air)
                        {
                            chunk[x, y, z] = Block.Water;
                        }
                    }
                }
                else
                {
                    // В других биомах заполняем водой только ниже уровня моря, если есть воздух
                    for (var y = chunk.MinY; y <= SeaLevel; y++)
                    {
                        if (chunk[x, y, z] == Block.Air)
                        {
                            // Проверяем, есть ли блоки выше (чтобы не создавать подвешенную воду)
                            var hasBlockAbove = false;
                            for (var checkY = y + 1; checkY <= SeaLevel; checkY++)
                            {
                                if (chunk[x, checkY, z] != Block.Air && chunk[x, checkY, z] != Block.Water)
                                {
                                    hasBlockAbove = true;
                                    break;
                                }
                            }
                            
                            if (!hasBlockAbove)
                            {
                                chunk[x, y, z] = Block.Water;
                            }
                        }
                    }
                }
            }
        }
    }
}
