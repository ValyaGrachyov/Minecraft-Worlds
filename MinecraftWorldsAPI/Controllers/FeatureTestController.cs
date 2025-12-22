using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace MinecraftWorldsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Feature Testing")]
public class FeatureTestController : ControllerBase
{
    private readonly IRandomFactory _randomFactory;
    private readonly IBiomeSource _biomeSource;
    private readonly IFeatureRegistry _featureRegistry;

    public FeatureTestController(
        IRandomFactory randomFactory,
        IBiomeSource biomeSource,
        IFeatureRegistry featureRegistry)
    {
        _randomFactory = randomFactory;
        _biomeSource = biomeSource;
        _featureRegistry = featureRegistry;
    }

    /// <summary>
    /// Тест генерации деревьев на чанке
    /// </summary>
    /// <param name="chunkX">Координата чанка X</param>
    /// <param name="chunkZ">Координата чанка Z</param>
    /// <param name="minY">Минимальная высота</param>
    /// <param name="maxY">Максимальная высота</param>
    /// <returns>Визуализация чанка и статистика</returns>
    [HttpGet("trees")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult TestTrees(
        [FromQuery] int chunkX = 0,
        [FromQuery] int chunkZ = 0,
        [FromQuery] int minY = 0,
        [FromQuery] int maxY = 20)
    {
        var chunkPos = new ChunkPos(chunkX, chunkZ);
        var chunk = new Chunk(chunkPos, minY, maxY);

        // создаём генератор для этого чанка
        var random = _randomFactory.CreateForChunk(0, chunkPos, 0);
        var context = new FeatureContext(chunk, _biomeSource, random);

        // применяем все фичи для каждого блока в чанке
        for (int x = 0; x < Chunk.SizeX; x++)
        for (int z = 0; z < Chunk.SizeZ; z++)
        {
            var biome = _biomeSource.GetBiome(chunkX * Chunk.SizeX + x, 0, chunkZ * Chunk.SizeZ + z);
            var features = _featureRegistry.GetFeaturesForBiome(biome);

            foreach (var feature in features)
                feature.Apply(context);
        }

        // визуализация сверху (x/z плоскость, только верхние блоки)
        var visualization = new StringBuilder();
        for (int z = 0; z < Chunk.SizeZ; z++)
        {
            for (int x = 0; x < Chunk.SizeX; x++)
            {
                // ищем верхний блок
                Block topBlock = Block.Air;
                for (int y = maxY; y >= minY; y--)
                {
                    if (chunk[x, y, z] != Block.Air)
                    {
                        topBlock = chunk[x, y, z];
                        break;
                    }
                }

                visualization.Append(BlockChar(topBlock));
            }
            visualization.AppendLine();
        }

        return Ok(new
        {
            chunk = new { chunkX, chunkZ, minY, maxY },
            visualization = visualization.ToString()
        });
    }

    private static char BlockChar(Block block) => block switch
    {
        Block.Air => ' ',
        Block.Dirt => '.',
        Block.Grass => '"',
        Block.Stone => '#',
        Block.Water => '~',
        Block.Log => '|',
        Block.Leaves => '*',
        _ => '?'
    };
}
