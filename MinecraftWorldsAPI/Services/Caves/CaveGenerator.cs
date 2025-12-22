using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Caves;

/// <summary>
/// Генератор пещер на основе 3D шумов Перлина
/// Реализует три типа пещер: сырные, спагетти и лапшичные, а также шумовые столбы
/// </summary>
public class CaveGenerator : ICaveGenerator
{
    // Масштабы для шумов пещер (влияют на размер и форму)
    private const double CaveXZScale = 0.05;  // Масштаб по X/Z
    private const double CaveYScale = 0.05;   // Масштаб по Y

    // Диапазон генерации пещер (от -59 до 256 по документации, но используем весь чанк)
    private const int CaveMinY = 0;
    private const int CaveMaxY = 255;

    public void Carve(Chunk chunk, IBiomeSource biomeSource, INoiseRegistry noiseRegistry)
    {
        var chunkPos = chunk.Position;
        var minX = chunkPos.X * Chunk.SizeX;
        var minZ = chunkPos.Z * Chunk.SizeZ;

        // Получаем шумы для разных типов пещер
        var cheeseNoise = noiseRegistry.GetNoise(NoiseNames.CaveCheese);
        var spaghettiNoise = noiseRegistry.GetNoise(NoiseNames.CaveSpaghetti);
        var noodleNoise = noiseRegistry.GetNoise(NoiseNames.CaveNoodle);
        var pillarsNoise = noiseRegistry.GetNoise(NoiseNames.CavePillars);

        // Проходим по всем блокам в чанке
        for (int x = 0; x < Chunk.SizeX; x++)
        {
            for (int z = 0; z < Chunk.SizeZ; z++)
            {
                var worldX = minX + x;
                var worldZ = minZ + z;

                for (int y = Math.Max(chunk.MinY, CaveMinY); y <= Math.Min(chunk.MaxY, CaveMaxY); y++)
                {
                    // Пропускаем воздушные блоки - пещеры создаются только в твердых блоках
                    if (chunk[x, y, z] == Block.Air)
                        continue;

                    var scaledX = worldX * CaveXZScale;
                    var scaledY = y * CaveYScale;
                    var scaledZ = worldZ * CaveXZScale;

                    // 1. Сырные пещеры (Cheese Caves)
                    // Белые области на шуме Перлина становятся воздушными
                    // Чем выше значение шума, тем больше вероятность пещеры
                    var cheeseValue = cheeseNoise.Sample(scaledX, scaledY, scaledZ);
                    var isCheeseCave = cheeseValue > NoiseDefaults.CaveCheeseHollowness;

                    // 2. Спагетти-пещеры (Spaghetti Caves)
                    // Края черно-белой части шума становятся воздушными
                    // Используем абсолютное значение для создания туннелей
                    var spaghettiValue = Math.Abs(spaghettiNoise.Sample(scaledX, scaledY, scaledZ));
                    var isSpaghettiCave = spaghettiValue < NoiseDefaults.CaveSpaghettiThickness;

                    // 3. Лапшичные пещеры (Noodle Caves)
                    // Более узкие и извилистые, чем спагетти
                    var noodleValue = Math.Abs(noodleNoise.Sample(scaledX, scaledY, scaledZ));
                    var isNoodleCave = noodleValue < NoiseDefaults.CaveNoodleThickness;

                    // Определяем, является ли блок частью пещеры
                    var isCave = isCheeseCave || isSpaghettiCave || isNoodleCave;

                    if (isCave)
                    {
                        // 4. Шумовые столбы (Noise Pillars)
                        // Генерируются внутри больших камер (сырных пещер)
                        // Если значение шума столбов низкое, создаем столб (камень)
                        if (isCheeseCave)
                        {
                            var pillarsValue = pillarsNoise.Sample(scaledX, scaledY, scaledZ);
                            // Если значение столбов очень низкое, оставляем камень (столб)
                            if (pillarsValue < -NoiseDefaults.CavePillarsThickness)
                            {
                                chunk[x, y, z] = Block.Stone;
                            }
                            else
                            {
                                chunk[x, y, z] = Block.Air;
                            }
                        }
                        else
                        {
                            // Для спагетти и лапшичных пещер просто делаем воздух
                            chunk[x, y, z] = Block.Air;
                        }
                    }
                }
            }
        }
    }
}

