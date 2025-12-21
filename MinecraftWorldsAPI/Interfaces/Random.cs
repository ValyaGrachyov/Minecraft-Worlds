using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Models.Enums;
using MinecraftWorldsAPI.Services.PRNG;

namespace MinecraftWorldsAPI.Interfaces;

public interface IRandom
{
    /// <summary>
    /// Set Seed value
    /// </summary>
    long Seed { get; }
    
    /// <summary>
    /// Generates random unsigned 32-bit value
    /// </summary>
    /// <returns><b>Random</b> value</returns>
    uint NextUInt();
    
    /// <summary>Returns a non-negative random integer.</summary>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue"/>.</returns>
    sealed int NextInt() => NextInt(0, int.MaxValue);

    /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
    /// <param name="max">The exclusive upper bound of the random number to be generated. <paramref name="max"/> must be greater than or equal to 0.</param>
    /// <returns>
    /// A 32-bit signed integer that is greater than or equal to 0, and less than <paramref name="max"/>; that is, the range of return values ordinarily
    /// includes 0 but not <paramref name="max"/>. However, if <paramref name="max"/> equals 0, <paramref name="max"/> is returned.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="max"/> is less than 0.</exception>
    sealed int NextInt(int max) => NextInt(0, max);

    /// <summary>Returns a random integer that is within a specified range.</summary>
    /// <param name="min">The inclusive lower bound of the random number returned.</param>
    /// <param name="max">The exclusive upper bound of the random number returned. <paramref name="max"/> must be greater than or equal to <paramref name="min"/>.</param>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to <paramref name="min"/> and less than <paramref name="max"/>; that is, the range of return values includes <paramref name="min"/>
    /// but not <paramref name="max"/>. If <paramref name="min"/> equals <paramref name="max"/>, <paramref name="min"/> is returned.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    int NextInt(int min, int max);

    /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
    /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    double NextDouble();

    /// <summary>
    /// Fork current generator into different one
    /// </summary>
    IRandom Fork(long salt);
}

public interface IRandomFactory
{
    IRandom CreateRandom(long seed, PrngType prngType = PrngType.XorShift64);

    IRandom CreateForChunk(long worldSeed, ChunkPos chunkPos, long salt, PrngType prngType = PrngType.XorShift64);
}
