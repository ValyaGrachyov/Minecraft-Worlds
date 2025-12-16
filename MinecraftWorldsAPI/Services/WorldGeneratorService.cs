using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services;

public class WorldGeneratorService : IWorldGenerator
{
    private readonly IRandomFactory _randomFactory;

    public WorldGeneratorService(IRandomFactory randomFactory)
    {
        _randomFactory = randomFactory;
    }

    public Chunk GenerateChunk(ChunkPos chunkPos, long seed)
    {
        var chunkRandom = _randomFactory.CreateForChunk(seed, chunkPos, salt:0);

        var terrain = chunkRandom.Fork(1);
        
        var chunk = new Chunk(chunkPos, 0, 10);
        
        return chunk;
    }
}