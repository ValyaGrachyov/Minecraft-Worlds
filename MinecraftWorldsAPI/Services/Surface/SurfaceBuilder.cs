using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Surface.Conditions;
using MinecraftWorldsAPI.Services.Surface.Rules;

namespace MinecraftWorldsAPI.Services.Surface;

/// <summary>
/// Генератор поверхности
/// Применяет правила поверхности для замены блоков после генерации базового ландшафта
/// </summary>
public class SurfaceBuilder : ISurfaceBuilder
{
    private readonly INoiseRegistry _noiseRegistry;
    private readonly IRandomFactory _randomFactory;
    private readonly ISurfaceRule _surfaceRules;

    // Масштабы для шумов поверхности
    private const double SurfaceXZScale = 0.05;

    public SurfaceBuilder(INoiseRegistry noiseRegistry, IRandomFactory randomFactory)
    {
        _noiseRegistry = noiseRegistry;
        _randomFactory = randomFactory;
        _surfaceRules = CreateDefaultSurfaceRules();
    }

    public void BuildSurface(Chunk chunk, IBiomeSource biomeSource)
    {
        var chunkPos = chunk.Position;
        var minX = chunkPos.X * Chunk.SizeX;
        var minZ = chunkPos.Z * Chunk.SizeZ;

        // Вычисляем surface depth для каждого столбца
        var surfaceDepths = new int[Chunk.SizeX, Chunk.SizeZ];
        var surfaceSecondaryDepths = new double[Chunk.SizeX, Chunk.SizeZ];

        for (int x = 0; x < Chunk.SizeX; x++)
        {
            for (int z = 0; z < Chunk.SizeZ; z++)
            {
                var worldX = minX + x;
                var worldZ = minZ + z;

                // Вычисляем surface depth: floor(surface(X,0,Z) × 2.75 + 3.0 + positional_noise(X,0,Z) × 0.25)
                var surfaceNoise = _noiseRegistry.GetNoise2D(NoiseNames.Surface)
                    .Sample(worldX * SurfaceXZScale, worldZ * SurfaceXZScale);
                
                var random = _randomFactory.CreateRandom((long)worldX * 1000 + worldZ);
                var positionalNoise = random.NextDouble(); // 0 to 1
                
                surfaceDepths[x, z] = (int)Math.Floor(surfaceNoise * 2.75 + 3.0 + positionalNoise * 0.25);

                // Вычисляем surface secondary depth
                var surfaceSecondaryNoise = _noiseRegistry.GetNoise2D(NoiseNames.SurfaceSecondary)
                    .Sample(worldX * SurfaceXZScale, worldZ * SurfaceXZScale);
                surfaceSecondaryDepths[x, z] = surfaceSecondaryNoise; // Уже в диапазоне [-1, 1]
            }
        }

        // Находим поверхность для каждого столбца (первый твердый блок сверху)
        var surfaceY = new int[Chunk.SizeX, Chunk.SizeZ];
        for (int x = 0; x < Chunk.SizeX; x++)
        {
            for (int z = 0; z < Chunk.SizeZ; z++)
            {
                surfaceY[x, z] = FindSurfaceY(chunk, x, z);
            }
        }

        // Проходим по всем блокам и применяем правила поверхности
        for (int x = 0; x < Chunk.SizeX; x++)
        {
            for (int z = 0; z < Chunk.SizeZ; z++)
            {
                var worldX = minX + x;
                var worldZ = minZ + z;
                var surfaceDepth = surfaceDepths[x, z];
                var surfaceSecondaryDepth = surfaceSecondaryDepths[x, z];
                var surfaceLevel = surfaceY[x, z];

                for (int y = chunk.MinY; y <= chunk.MaxY; y++)
                {
                    // Пропускаем воздушные блоки
                    if (chunk[x, y, z] == Block.Air)
                        continue;

                    var biome = biomeSource.GetBiome(worldX, y, worldZ);

                    // Вычисляем stone depth (расстояние до поверхности)
                    var stoneDepthAbove = surfaceLevel - y;
                    var stoneDepthBelow = y - FindBottomSurfaceY(chunk, x, z);

                    // Определяем наличие воды
                    int? waterHeight = FindWaterHeight(chunk, x, z, y);

                    // Определяем потолок (есть ли твердый блок сверху)
                    var isCeiling = y < chunk.MaxY && chunk[x, y + 1, z] != Block.Air;

                    // Создаем контекст
                    var context = new SurfaceContext(worldX, y, worldZ, biome)
                    {
                        SurfaceDepth = surfaceDepth,
                        SurfaceSecondaryDepth = surfaceSecondaryDepth,
                        StoneDepthAbove = stoneDepthAbove,
                        StoneDepthBelow = stoneDepthBelow,
                        WaterHeight = waterHeight,
                        IsCeiling = isCeiling
                    };

                    // Применяем правила поверхности
                    var newBlock = _surfaceRules.Apply(context);
                    if (newBlock.HasValue)
                    {
                        chunk[x, y, z] = newBlock.Value;
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
            if (chunk[x, y, z] != Block.Air)
            {
                return y;
            }
        }
        return chunk.MinY;
    }

    /// <summary>
    /// Находит Y координату нижней поверхности (первый твердый блок снизу)
    /// </summary>
    private static int FindBottomSurfaceY(Chunk chunk, int x, int z)
    {
        for (int y = chunk.MinY; y <= chunk.MaxY; y++)
        {
            if (chunk[x, y, z] != Block.Air)
            {
                return y;
            }
        }
        return chunk.MaxY;
    }

    /// <summary>
    /// Находит высоту воды над блоком
    /// </summary>
    private static int? FindWaterHeight(Chunk chunk, int x, int z, int blockY)
    {
        int waterCount = 0;
        for (int y = blockY + 1; y <= chunk.MaxY; y++)
        {
            if (chunk[x, y, z] == Block.Water)
            {
                waterCount++;
            }
            else if (chunk[x, y, z] != Block.Air)
            {
                break; // Встретили твердый блок
            }
        }
        return waterCount > 0 ? waterCount : null;
    }

    /// <summary>
    /// Создает правила поверхности по умолчанию
    /// Основано на таблицах из документации
    /// </summary>
    private ISurfaceRule CreateDefaultSurfaceRules()
    {
        // Получаем шумы для условий
        var surfaceNoise = _noiseRegistry.GetNoise2D(NoiseNames.Surface);

        // Базовые правила для разных биомов
        var rules = new List<ISurfaceRule>();

        // Plains: Grass Block на поверхности, Dirt под ней
        rules.Add(new ConditionRule(
            new BiomeCondition(Biome.Plains),
            new SequenceRule(
                new ConditionRule(
                    new StoneDepthCondition(StoneDepthCondition.SurfaceType.Floor, 0),
                    new ConditionRule(
                        new NotCondition(new WaterCondition()),
                        new BlockRule(Block.Grass)
                    )
                ),
                new ConditionRule(
                    new StoneDepthCondition(StoneDepthCondition.SurfaceType.Floor, 3),
                    new BlockRule(Block.Dirt)
                )
            )
        ));

        // Desert: Sand на поверхности
        rules.Add(new ConditionRule(
            new BiomeCondition(Biome.Desert),
            new SequenceRule(
                new ConditionRule(
                    new StoneDepthCondition(StoneDepthCondition.SurfaceType.Floor, 0),
                    new BlockRule(Block.Dirt) // Используем Dirt как замену Sand (так как Sand нет в enum)
                )
            )
        ));

        // Forest: Grass Block на поверхности, Dirt под ней
        rules.Add(new ConditionRule(
            new BiomeCondition(Biome.Forest),
            new SequenceRule(
                new ConditionRule(
                    new StoneDepthCondition(StoneDepthCondition.SurfaceType.Floor, 0),
                    new ConditionRule(
                        new NotCondition(new WaterCondition()),
                        new BlockRule(Block.Grass)
                    )
                ),
                new ConditionRule(
                    new StoneDepthCondition(StoneDepthCondition.SurfaceType.Floor, 3),
                    new BlockRule(Block.Dirt)
                )
            )
        ));

        // Mountains: Stone на поверхности
        rules.Add(new ConditionRule(
            new BiomeCondition(Biome.ExtremeHills),
            new SequenceRule(
                new ConditionRule(
                    new StoneDepthCondition(StoneDepthCondition.SurfaceType.Floor, 0),
                    new BlockRule(Block.Stone)
                )
            )
        ));

        // Ocean: Dirt под водой
        rules.Add(new ConditionRule(
            new BiomeCondition(Biome.Ocean),
            new SequenceRule(
                new ConditionRule(
                    new WaterCondition(),
                    new ConditionRule(
                        new StoneDepthCondition(StoneDepthCondition.SurfaceType.Floor, 5),
                        new BlockRule(Block.Dirt)
                    )
                )
            )
        ));

        return new SequenceRule(rules);
    }
}

