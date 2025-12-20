namespace MinecraftWorldsAPI.Interfaces.noise;

public interface INoise2D
{
    double Sample(double x, double y);
}

public sealed class ValueNoise2D : INoise2D
{
    private readonly long seed;
    private readonly double frequency;
    private readonly IRandomFactory randomFactory;

    public ValueNoise2D(
        IRandomFactory randomFactory,
        long seed,
        double frequency
    )
    {
        this.randomFactory = randomFactory;
        this.seed = seed;
        this.frequency = frequency;
    }

    public double Sample(double x, double z)
    {
        x *= frequency;
        z *= frequency;

        int x0 = Floor(x);
        int z0 = Floor(z);

        int x1 = x0 + 1;
        int z1 = z0 + 1;

        double tx = x - x0;
        double tz = z - z0;

        double v00 = RandomValue(x0, z0);
        double v10 = RandomValue(x1, z0);
        double v01 = RandomValue(x0, z1);
        double v11 = RandomValue(x1, z1);

        double sx = Smooth(tx);
        double sz = Smooth(tz);

        double ix0 = Lerp(v00, v10, sx);
        double ix1 = Lerp(v01, v11, sx);

        return Lerp(ix0, ix1, sz); // [-1;1]
    }

    private double RandomValue(int x, int z)
    {
        long salt = Hash(x, z);
        var random = randomFactory.CreateRandom(seed ^ salt);
        return random.NextDouble() * 2.0 - 1.0;
    }

    private static long Hash(int x, int z)
    {
        unchecked
        {
            long h = x;
            h = h * 31 + z;
            h ^= h >> 33;
            h *= 0xff51af;
            h ^= h >> 33;
            return h;
        }
    }

    private static int Floor(double v) => v >= 0 ? (int)v : (int)v - 1;

    private static double Lerp(double a, double b, double t)
        => a + (b - a) * t;

    private static double Smooth(double t)
        => t * t * (3.0 - 2.0 * t);
}
