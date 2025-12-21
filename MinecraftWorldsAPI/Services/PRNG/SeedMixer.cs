namespace MinecraftWorldsAPI.Services.PRNG;

public static class SeedMixer
{
    public static long Mix(long seed, long salt)
    {
        unchecked
        {
            long h = seed;
            h ^= salt + (long) 0x9E3779B97F4A7C15;
            h *= (long) 0xBF58476D1CE4E5B9;
            h ^= h >> 27;
            h *= (long) 0x94D049BB133111EB;
            h ^= h >> 31;
            return h;
        }
    }

    public static long Mix(long seed, int x, int z)
    {
        unchecked
        {
            long h = seed;
            h ^= x * 341873128712L;
            h ^= z * 132897987541L;
            return h;
        }
    }
}

