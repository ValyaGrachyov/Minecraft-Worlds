using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Models.Enums;

namespace MinecraftWorldsAPI.Controllers;

[ApiController]
[Route("api/prng/[action]")]
[Tags("Prng Testing")]
public class PrngTestController : ControllerBase
{
    private readonly IRandomFactoryWithType _randomFactoryWithType;

    public PrngTestController(IRandomFactoryWithType randomFactory)
    {
        _randomFactoryWithType = randomFactory;
    }

    // -----------------------------
    // Public endpoints
    // -----------------------------

    /// <summary>
    /// Проверка CreateRandom (глобальный PRNG)
    /// </summary>
    [HttpGet("random")]
    public IActionResult TestRandom(
        [FromQuery] long seed = 123,
        [FromQuery] int count = 10,
        [FromQuery] PrngType type = PrngType.XorShift64
    )
    {
        ValidateCount(count);

        var rnd = _randomFactoryWithType.CreateRandom(seed, type);
        return Ok(BuildResponse(rnd, seed, count));
    }

    /// <summary>
    /// Проверка CreateForChunk
    /// </summary>
    [HttpGet("chunk")]
    public IActionResult TestChunkRandom(
        [FromQuery] long seed = 123,
        [FromQuery] int x = 0,
        [FromQuery] int z = 0,
        [FromQuery] long salt = 0,
        [FromQuery] int count = 10,
        [FromQuery] PrngType type = PrngType.XorShift64
    )
    {
        ValidateCount(count);

        var chunkPos = new ChunkPos(x, z);
        var rnd = _randomFactoryWithType.CreateForChunk(seed, chunkPos, salt, type);

        return Ok(BuildResponse(rnd, rnd.Seed, count));
    }

    /// <summary>
    /// Проверка Fork
    /// </summary>
    [HttpGet("fork")]
    public IActionResult TestFork(
        [FromQuery] long seed = 123,
        [FromQuery] int x = 0,
        [FromQuery] int z = 0,
        [FromQuery] long salt = 0,
        [FromQuery] long forkSalt = 42,
        [FromQuery] int count = 10,
        [FromQuery] PrngType type = PrngType.XorShift64
    )
    {
        ValidateCount(count);

        var chunkPos = new ChunkPos(x, z);

        var baseRandom = _randomFactoryWithType.CreateForChunk(seed, chunkPos, salt, type);
        var forked = baseRandom.Fork(forkSalt);

        return Ok(BuildResponse(forked, forked.Seed, count));
    }

    /// <summary>
    /// Список доступных PRNG алгоритмов
    /// </summary>
    [HttpGet("algorithms")]
    public IActionResult GetAlgorithms()
    {
        var values = Enum.GetValues<PrngType>()
            .Select(t => new
            {
                Id = t,
                Name = t.ToString()
            });

        return Ok(values);
    }

    // -----------------------------
    // Helpers
    // -----------------------------

    private static PrngSampleResponse BuildResponse(
        IRandom random,
        long seed,
        int count)
    {
        var values = new int[count];

        for (int i = 0; i < count; i++)
            values[i] = random.NextInt(0, 100);

        return new PrngSampleResponse(
            Algorithm: random.GetType().Name,
            Seed: seed,
            Values: values
        );
    }

    private static void ValidateCount(int count)
    {
        if (count <= 0 || count > 10_000)
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "Count must be between 1 and 10_000"
            );
    }
}

public record PrngSampleResponse(
    string Algorithm,
    long Seed,
    int[] Values
);