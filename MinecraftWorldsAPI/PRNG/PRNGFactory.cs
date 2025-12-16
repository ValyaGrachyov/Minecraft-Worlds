using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.PRNG;

namespace MinecraftWorldsAPI.PRNG;

public class PRNGFactory : IRandomFactory
{
    public IRandom CreateRandom(long seed)
    {
        return new XorShift64Random(seed);
    }

    public IRandom CreateForChunk(long worldSeed, ChunkPos chunkPos, long salt)
    {
        long chunkSeed = SeedMixer.Mix(
            worldSeed,
            chunkPos.X,
            chunkPos.Z
        );

        chunkSeed = SeedMixer.Mix(chunkSeed, salt);

        return new XorShift64Random(chunkSeed);
    }
}