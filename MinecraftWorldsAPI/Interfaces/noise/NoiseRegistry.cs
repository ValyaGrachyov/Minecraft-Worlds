using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Interfaces.noise;

public interface INoiseRegistry
{
    INoise GetNoise(string name);
    INoise2D GetNoise2D(string name);
}