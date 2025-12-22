using Microsoft.AspNetCore.Mvc;
using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Models.Enums;
using MinecraftWorldsAPI.Services;

namespace MinecraftWorldsAPI.Controllers;

[ApiController]
[Route("generate")]
[Tags("Main Generator")]
public class GenerateController(IRandomFactory randomFactory, IWorldGenerator worldGenerator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Generate(
        [FromQuery] string? seed,
        [FromQuery] int coordX,
        [FromQuery] int coordZ,
        [FromQuery] int chunks,
        [FromQuery] PrngType algorythm = PrngType.XorShift64,
        [FromQuery] string worldName = "WorldName",
        CancellationToken ct = default)
    {
        randomFactory.Type = algorythm;
        int chunkX = coordX / Chunk.SizeX,
            chunkZ = coordZ / Chunk.SizeZ;

        var seedLong = seed?.GetHashCode() ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var res = new List<Chunk>();
        for (var x = chunkX - chunks; x <= chunkX + chunks; x++)
        for (var z = chunkZ - chunks; z <= chunkZ + chunks; z++)
            res.Add(worldGenerator.GenerateChunk(new ChunkPos(x, z), seedLong));

        var ms = new MemoryStream();
        await Converter.ConvertAsync(ms, res, new WorldExportOption(worldName, Seed: seedLong), ct);
        ms.Position = 0;

        return File(ms, "application/zip", $"{worldName}.zip");
    }
}
