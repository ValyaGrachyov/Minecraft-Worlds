using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.PRNG;

namespace MinecraftWorldsAPI.Services.PRNG;

public sealed class XorShift64Random : IRandom
{
    private ulong _state;

    public long Seed { get; }

    public XorShift64Random(long seed)
    {
        Seed = seed;
        _state = (ulong)seed;

        if (_state == 0)
            _state = 0xdeadbeefcafebabeUL;
    }

    public uint NextUInt()
    {
        ulong x = _state;
        x ^= x >> 12;
        x ^= x << 25;
        x ^= x >> 27;
        _state = x;
        return (uint)((x * 2685821657736338717UL) >> 32);
    }

    public int NextInt(int min, int max)
    {
        if (min > max)
            throw new ArgumentOutOfRangeException();

        if (min == max)
            return min;

        uint range = (uint)(max - min);
        return (int)(NextUInt() % range) + min;
    }

    public double NextDouble()
    {
        return NextUInt() / (double)uint.MaxValue;
    }

    public IRandom Fork(long salt)
    {
        long forkSeed = SeedMixer.Mix(Seed, salt);
        return new XorShift64Random(forkSeed);
    }
}
