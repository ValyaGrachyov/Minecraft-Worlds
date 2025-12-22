using Microsoft.AspNetCore.Mvc;
using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Models.Enums;

namespace MinecraftWorldsAPI.Controllers;

/// <summary>
/// Контроллер для генерации чанков
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChunkController(IRandomFactory randomFactory, IWorldGenerator worldGenerator) : ControllerBase
{
    /// <summary>
    /// Генерирует чанк по указанным координатам и сиду
    /// </summary>
    /// <param name="x">X координата чанка</param>
    /// <param name="z">Z координата чанка</param>
    /// <param name="seed">Сид мира (опционально, по умолчанию 0)</param>
    /// <param name="type">Тип генератора случайных чисел (по умолчанию: XorShift64)</param>
    /// <returns>Сгенерированный чанк</returns>
    [HttpGet("{x}/{z}")]
    [ProducesResponseType(typeof(ChunkResponse), StatusCodes.Status200OK)]
    public ActionResult<ChunkResponse> GenerateChunk(int x, int z, [FromQuery] long seed = 0, [FromQuery] PrngType type = PrngType.XorShift64)
    {
        randomFactory.Type = type;

        var chunkPos = new ChunkPos(x, z);
        var chunk = worldGenerator.GenerateChunk(chunkPos, seed);
        
        return Ok(new ChunkResponse
        {
            Position = chunkPos,
            MinY = chunk.MinY,
            MaxY = chunk.MaxY,
            Blocks = ConvertChunkToArray(chunk)
        });
    }
    
    private static Block[,,] ConvertChunkToArray(Chunk chunk)
    {
        var blocks = new Block[Chunk.SizeX, chunk.MaxY - chunk.MinY + 1, Chunk.SizeZ];
        
        for (var x = 0; x < Chunk.SizeX; x++)
        {
            for (var y = chunk.MinY; y <= chunk.MaxY; y++)
            {
                for (var z = 0; z < Chunk.SizeZ; z++)
                {
                    blocks[x, y - chunk.MinY, z] = chunk[x, y, z].Block;
                }
            }
        }
        
        return blocks;
    }
}

/// <summary>
/// Ответ с данными чанка
/// </summary>
public class ChunkResponse
{
    public ChunkPos Position { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }
    public Block[,,] Blocks { get; set; } = null!;
}
