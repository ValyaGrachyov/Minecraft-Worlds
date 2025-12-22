using Microsoft.AspNetCore.Mvc;
using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Models.Enums;

namespace MinecraftWorldsAPI.Controllers;

/// <summary>
/// Контроллер для тестирования генерации мира (terrain, caves, fluids, surface)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("World Generation Testing")]
public class WorldGenerationTestController(
    IRandomFactory randomFactory,
    IDensityFunction densityFunction,
    ITerrainGenerator terrainGenerator,
    ICaveGenerator caveGenerator,
    IFluidFiller fluidFiller,
    ISurfaceBuilder surfaceBuilder,
    IBiomeSource biomeSource,
    INoiseRegistry noiseRegistry)
    : ControllerBase
{
    /// <summary>
    /// Генерирует чанк и возвращает полную статистику по всем этапам
    /// </summary>
    [HttpGet("chunk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GenerateChunk(
        [FromQuery] int chunkX = 0,
        [FromQuery] int chunkZ = 0,
        [FromQuery] long seed = 12345,
        [FromQuery] PrngType type = PrngType.XorShift64)
    {
        randomFactory.Type = type;
        var chunkPos = new ChunkPos(chunkX, chunkZ);
        var chunk = new Chunk(chunkPos, -64, 320);

        // Этап 1: Генерация базового ландшафта
        var statsAfterTerrain = GenerateAndGetStats(chunk, "Terrain", () =>
        {
            terrainGenerator.GenerateBaseTerrain(chunk, densityFunction, biomeSource);
        });

        // Этап 2: Генерация пещер
        var statsAfterCaves = GenerateAndGetStats(chunk, "Caves", () =>
        {
            caveGenerator.Carve(chunk, biomeSource, noiseRegistry);
        });

        // Этап 3: Заполнение жидкостями
        var statsAfterFluids = GenerateAndGetStats(chunk, "Fluids", () =>
        {
            fluidFiller.FillFluids(chunk, biomeSource);
        });

        // Этап 4: Построение поверхности
        var statsAfterSurface = GenerateAndGetStats(chunk, "Surface", () =>
        {
            surfaceBuilder.BuildSurface(chunk, biomeSource);
        });

        // Визуализация среза на уровне Y=64
        var sliceY = 64;
        var visualization = GenerateSliceVisualization(chunk, sliceY);

        return Ok(new
        {
            chunkPosition = new { chunkX, chunkZ },
            seed,
            stages = new[]
            {
                statsAfterTerrain,
                statsAfterCaves,
                statsAfterFluids,
                statsAfterSurface
            },
            finalStats = statsAfterSurface,
            sliceVisualization = new
            {
                y = sliceY,
                visualization = visualization.visualization,
                legend = visualization.legend
            },
            message = "Чанк успешно сгенерирован со всеми этапами"
        });
    }

    /// <summary>
    /// Генерирует вертикальный срез чанка (X=8, все Y и Z)
    /// </summary>
    [HttpGet("vertical-slice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GenerateVerticalSlice(
        [FromQuery] int chunkX = 0,
        [FromQuery] int chunkZ = 0,
        [FromQuery] int sliceX = 8,
        [FromQuery] long seed = 12345,
        [FromQuery] PrngType type = PrngType.XorShift64)
    {
        randomFactory.Type = type;
        var chunkPos = new ChunkPos(chunkX, chunkZ);
        var chunk = new Chunk(chunkPos, -64, 320);

        // Генерируем весь чанк
        terrainGenerator.GenerateBaseTerrain(chunk, densityFunction, biomeSource);
        caveGenerator.Carve(chunk, biomeSource, noiseRegistry);
        fluidFiller.FillFluids(chunk, biomeSource);
        surfaceBuilder.BuildSurface(chunk, biomeSource);

        // Генерируем визуализацию вертикального среза
        var visualization = new System.Text.StringBuilder();
        var legend = GetBlockLegend();

        // Сверху вниз (от MaxY к MinY)
        for (int y = chunk.MaxY; y >= chunk.MinY; y--)
        {
            visualization.Append($"{y,4} | ");
            for (int z = 0; z < Chunk.SizeZ; z++)
            {
                var block = chunk[sliceX, y, z];
                visualization.Append(GetBlockChar(block));
            }
            visualization.AppendLine();
        }

        return Ok(new
        {
            chunkPosition = new { chunkX, chunkZ },
            sliceX,
            seed,
            visualization = visualization.ToString(),
            legend
        });
    }

    /// <summary>
    /// Генерирует горизонтальный срез чанка на указанной высоте
    /// </summary>
    [HttpGet("horizontal-slice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GenerateHorizontalSlice(
        [FromQuery] int chunkX = 0,
        [FromQuery] int chunkZ = 0,
        [FromQuery] int sliceY = 64,
        [FromQuery] long seed = 12345,
        [FromQuery] PrngType type = PrngType.XorShift64)
    {
        randomFactory.Type = type;
        var chunkPos = new ChunkPos(chunkX, chunkZ);
        var chunk = new Chunk(chunkPos, -64, 320);

        // Генерируем весь чанк
        terrainGenerator.GenerateBaseTerrain(chunk, densityFunction, biomeSource);
        caveGenerator.Carve(chunk, biomeSource, noiseRegistry);
        fluidFiller.FillFluids(chunk, biomeSource);
        surfaceBuilder.BuildSurface(chunk, biomeSource);

        var visualization = GenerateSliceVisualization(chunk, sliceY);

        return Ok(new
        {
            chunkPosition = new { chunkX, chunkZ },
            sliceY,
            seed,
            visualization = visualization.visualization,
            legend = visualization.legend
        });
    }

    /// <summary>
    /// Статистика по блокам на разных этапах генерации
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetGenerationStats(
        [FromQuery] int chunkX = 0,
        [FromQuery] int chunkZ = 0,
        [FromQuery] long seed = 12345,
        [FromQuery] PrngType type = PrngType.XorShift64)
    {
        randomFactory.Type = type;
        var chunkPos = new ChunkPos(chunkX, chunkZ);
        var chunk = new Chunk(chunkPos, -64, 320);

        var stages = new List<object>();

        // После terrain
        terrainGenerator.GenerateBaseTerrain(chunk, densityFunction, biomeSource);
        stages.Add(new
        {
            stage = "After Terrain",
            blockCounts = GetBlockCounts(chunk)
        });

        // После caves
        caveGenerator.Carve(chunk, biomeSource, noiseRegistry);
        stages.Add(new
        {
            stage = "After Caves",
            blockCounts = GetBlockCounts(chunk)
        });

        // После fluids
        fluidFiller.FillFluids(chunk, biomeSource);
        stages.Add(new
        {
            stage = "After Fluids",
            blockCounts = GetBlockCounts(chunk)
        });

        // После surface
        surfaceBuilder.BuildSurface(chunk, biomeSource);
        stages.Add(new
        {
            stage = "After Surface",
            blockCounts = GetBlockCounts(chunk)
        });

        return Ok(new
        {
            chunkPosition = new { chunkX, chunkZ },
            seed,
            stages
        });
    }

    /// <summary>
    /// Вспомогательный метод для генерации и получения статистики
    /// </summary>
    private object GenerateAndGetStats(Chunk chunk, string stageName, Action generateAction)
    {
        generateAction();
        return new
        {
            stage = stageName,
            blockCounts = GetBlockCounts(chunk),
            totalBlocks = Chunk.SizeX * Chunk.SizeZ * (chunk.MaxY - chunk.MinY + 1)
        };
    }

    /// <summary>
    /// Подсчитывает количество блоков каждого типа
    /// </summary>
    private Dictionary<string, int> GetBlockCounts(Chunk chunk)
    {
        var counts = new Dictionary<string, int>();

        for (int x = 0; x < Chunk.SizeX; x++)
        {
            for (int z = 0; z < Chunk.SizeZ; z++)
            {
                for (int y = chunk.MinY; y <= chunk.MaxY; y++)
                {
                    var block = chunk[x, y, z];
                    var blockName = block.ToString();

                    if (!counts.ContainsKey(blockName))
                        counts[blockName] = 0;

                    counts[blockName]++;
                }
            }
        }

        return counts;
    }

    /// <summary>
    /// Генерирует визуализацию горизонтального среза
    /// </summary>
    private (string visualization, Dictionary<string, string> legend) GenerateSliceVisualization(Chunk chunk, int sliceY)
    {
        var visualization = new System.Text.StringBuilder();
        var legend = GetBlockLegend();

        // Проверяем, что Y в пределах чанка
        if (sliceY < chunk.MinY || sliceY > chunk.MaxY)
        {
            visualization.AppendLine($"Y={sliceY} вне диапазона чанка [{chunk.MinY}, {chunk.MaxY}]");
            return (visualization.ToString(), legend);
        }

        visualization.AppendLine($"Срез чанка на Y={sliceY}:");
        visualization.AppendLine("   " + string.Join("", Enumerable.Range(0, Chunk.SizeZ).Select(i => i % 10)));

        for (int x = 0; x < Chunk.SizeX; x++)
        {
            visualization.Append($"{x % 10} ");
            for (int z = 0; z < Chunk.SizeZ; z++)
            {
                var block = chunk[x, sliceY, z];
                visualization.Append(GetBlockChar(block));
            }
            visualization.AppendLine();
        }

        return (visualization.ToString(), legend);
    }

    /// <summary>
    /// Возвращает символ для визуализации блока
    /// </summary>
    private static char GetBlockChar(Block block)
    {
        return block switch
        {
            Block.Air => ' ',
            Block.Stone => '#',
            Block.Dirt => '=',
            Block.Grass => '+',
            Block.Water => '~',
            _ => '?'
        };
    }

    /// <summary>
    /// Возвращает легенду для блоков
    /// </summary>
    private static Dictionary<string, string> GetBlockLegend()
    {
        return new Dictionary<string, string>
        {
            [" "] = "Air",
            ["#"] = "Stone",
            ["="] = "Dirt",
            ["+"] = "Grass",
            ["~"] = "Water"
        };
    }
}

