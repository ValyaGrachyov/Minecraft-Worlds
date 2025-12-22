namespace MinecraftWorldsAPI.Interfaces;

public interface INoise
{
    double Sample(double x, double y, double z);
}

public interface INoise2D
{
    double Sample(double x, double y);
}

public interface INoiseRegistry
{
    INoise GetNoise(string name);
    INoise2D GetNoise2D(string name);
}
