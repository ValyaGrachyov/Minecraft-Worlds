using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Fluids;

/// <summary>
/// Заполнитель жидкостями
/// Заполняет океаны и подземные полости водой
/// </summary>
public class FluidFiller : IFluidFiller
{
    // Уровень моря (sea level)
    private const int SeaLevel = 63;

    // Минимальная глубина для заполнения подземных полостей водой
    private const int UndergroundWaterMinY = 0;

    public void FillFluids(Chunk chunk, IBiomeSource biomeSource)
    {
        var chunkPos = chunk.Position;
        var minX = chunkPos.X * Chunk.SizeX;
        var minZ = chunkPos.Z * Chunk.SizeZ;

        // Проходим по всем столбцам в чанке
        for (int x = 0; x < Chunk.SizeX; x++)
        {
            for (int z = 0; z < Chunk.SizeZ; z++)
            {
                var worldX = minX + x;
                var worldZ = minZ + z;

                // Определяем биом для этого столбца (используем средний Y)
                var biome = biomeSource.GetBiome(worldX, SeaLevel, worldZ);

                // Находим поверхность (первый твердый блок сверху)
                var surfaceY = FindSurfaceY(chunk, x, z);

                if (biome == Biome.Ocean)
                {
                    // Для океанов заполняем водой до уровня моря
                    FillOcean(chunk, x, z, surfaceY);
                }
                else
                {
                    // Для других биомов заполняем подземные полости водой ниже уровня моря
                    FillUndergroundCavities(chunk, x, z, surfaceY);
                }
            }
        }
    }

    /// <summary>
    /// Заполняет океан водой до уровня моря
    /// </summary>
    private static void FillOcean(Chunk chunk, int x, int z, int surfaceY)
    {
        // Заполняем водой от дна до уровня моря или до поверхности (что ниже)
        var fillTop = Math.Min(SeaLevel, chunk.MaxY);
        var fillBottom = Math.Max(chunk.MinY, UndergroundWaterMinY);

        for (int y = fillBottom; y <= fillTop; y++)
        {
            // Заполняем только воздушные блоки
            if (chunk[x, y, z] == Block.Air)
            {
                chunk[x, y, z] = Block.Water;
            }
        }
    }

    /// <summary>
    /// Заполняет подземные полости водой ниже уровня моря
    /// Заполняет все воздушные блоки ниже уровня моря, которые находятся под поверхностью
    /// </summary>
    private static void FillUndergroundCavities(Chunk chunk, int x, int z, int surfaceY)
    {
        // Заполняем только полости ниже уровня моря
        if (surfaceY >= SeaLevel)
            return;

        var fillTop = Math.Min(SeaLevel - 1, chunk.MaxY);
        var fillBottom = Math.Max(chunk.MinY, UndergroundWaterMinY);

        // Заполняем водой все воздушные блоки ниже уровня моря
        // Вода будет заполнять полости естественным образом (сверху вниз)
        for (int y = fillTop; y >= fillBottom; y--)
        {
            if (chunk[x, y, z] == Block.Air)
            {
                // Проверяем, есть ли вода выше или это первый блок под поверхностью
                // Если есть вода выше, заполняем текущий блок водой
                var hasWaterAbove = y < fillTop && chunk[x, y + 1, z] == Block.Water;
                var isBelowSurface = y < surfaceY;

                if (hasWaterAbove || isBelowSurface)
                {
                    // Проверяем, что снизу есть твердый блок (чтобы вода не падала в пустоту)
                    var hasSolidBelow = y > fillBottom && chunk[x, y - 1, z] != Block.Air && chunk[x, y - 1, z] != Block.Water;
                    
                    // Заполняем водой, если есть опора снизу или это самый нижний блок
                    if (hasSolidBelow || y == fillBottom)
                    {
                        chunk[x, y, z] = Block.Water;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Находит Y координату поверхности (первый твердый блок сверху)
    /// </summary>
    private static int FindSurfaceY(Chunk chunk, int x, int z)
    {
        for (int y = chunk.MaxY; y >= chunk.MinY; y--)
        {
            if (chunk[x, y, z] != Block.Air && chunk[x, y, z] != Block.Water)
            {
                return y;
            }
        }
        return chunk.MinY;
    }
}

