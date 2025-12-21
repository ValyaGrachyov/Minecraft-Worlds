using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Models.Enums;

namespace MinecraftWorldsAPI.Services.PRNG;

public class PrngFactory : IRandomFactory
{
    public IRandom CreateRandom(long seed, PrngType prngType)
        => Create(seed, prngType);

    public IRandom CreateForChunk(
        long worldSeed,
        ChunkPos chunkPos,
        long salt,
        PrngType prngType)
    {
        long s = SeedMixer.Mix(worldSeed, chunkPos.X, chunkPos.Z);
        s = SeedMixer.Mix(s, salt);

        return Create(s, prngType);
    }

    private static IRandom Create(long seed, PrngType prngType)
        => prngType switch
    {
        PrngType.MersenneTwister => new MersenneTwisterRandom(seed),
        _ => new XorShift64Random(seed)
    };
}