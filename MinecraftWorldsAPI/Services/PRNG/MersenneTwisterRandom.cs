using MinecraftWorldsAPI.Interfaces;

namespace MinecraftWorldsAPI.Services.PRNG;

public class MersenneTwisterRandom : IRandom
{
    private const int N = 624;
    private const int M = 397;
    private const uint MATRIX_A = 0x9908B0DF;
    private const uint UPPER_MASK = 0x80000000;
    private const uint LOWER_MASK = 0x7FFFFFFF;

    private readonly uint[] _mt = new uint[N];
    private int _index;

    public long Seed { get; }

    public MersenneTwisterRandom(long seed)
    {
        Seed = seed;
        Initialize((uint)seed);
    }

    private void Initialize(uint seed)
    {
        _mt[0] = seed;
        for (int i = 1; i < N; i++)
        {
            _mt[i] = 1812433253u * (_mt[i - 1] ^ (_mt[i - 1] >> 30)) + (uint)i;
        }

        _index = N;
    }

    private void Twist()
    {
        for (int i = 0; i < N; i++)
        {
            uint y = (_mt[i] & UPPER_MASK) | (_mt[(i + 1) % N] & LOWER_MASK);
            _mt[i] = _mt[(i + M) % N] ^ (y >> 1);

            if ((y & 1) != 0)
                _mt[i] ^= MATRIX_A;
        }

        _index = 0;
    }

    public uint NextUInt()
    {
        if (_index >= N)
            Twist();

        uint y = _mt[_index++];

        // Tempering
        y ^= y >> 11;
        y ^= (y << 7) & 0x9D2C5680;
        y ^= (y << 15) & 0xEFC60000;
        y ^= y >> 18;

        return y;
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
        return new MersenneTwisterRandom(forkSeed);
    }
}