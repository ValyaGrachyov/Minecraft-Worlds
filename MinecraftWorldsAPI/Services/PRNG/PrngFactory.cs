using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Models.Enums;

namespace MinecraftWorldsAPI.Services.PRNG;

public class PrngFactory : IRandomFactory
{
    public PrngType Type { get; set; } = PrngType.XorShift64;

    public IRandom CreateRandom(long seed)
        => Create(seed);

    public IRandom CreateForChunk(
        long worldSeed,
        ChunkPos chunkPos,
        long salt)
    {
        long s = SeedMixer.Mix(worldSeed, chunkPos.X, chunkPos.Z);
        s = SeedMixer.Mix(s, salt);

        return Create(s);
    }

    private IRandom Create(long seed)
        => Type switch
    {
        PrngType.MersenneTwister => new MersenneTwisterRandom(seed),
        _ => new XorShift64Random(seed)
    };
}