using Microsoft.AspNetCore.Mvc;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services;

namespace MinecraftWorldsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Converter Testing")]
public class ConverterTestController : ControllerBase
{
    [HttpGet("converter")]
    public async Task<IActionResult> TestConverter([FromQuery] string worldName, CancellationToken ct)
    {
        HashSet<int> lags = [
            8, // water
            9, // water
            10, // lava
            11, // lava
            34, // piston top
            212, // ice
            219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, // shulkers
        
        ];

        var chunk = new Chunk(new ChunkPos(0, 0));
        for (var x = 0; x < Chunk.SizeX; x++)
        for (var z = 0; z < Chunk.SizeZ; z++)
        for (var y = Chunk.DefaultMinY; y < Chunk.DefaultMaxY; y++)
            chunk[x, y, z] = lags.Contains(y) || Random.Shared.Next() % 2 == 0 ? Block.Air : (Block)y;

        var ms = new MemoryStream();
        await Converter.ConvertAsync(ms, [chunk], new WorldExportOption(worldName, Seed: 51651), ct);
        ms.Position = 0;
    
        return File(ms, "application/zip", $"{worldName}.zip");
    }
}
