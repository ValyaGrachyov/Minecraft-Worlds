using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Interfaces;

public interface ICaveGenerator
{
    void Carve(Chunk chunk, IBiomeSource biomeSource, INoiseRegistry noiseRegistry);
}
