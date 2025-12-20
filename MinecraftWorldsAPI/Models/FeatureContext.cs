using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Interfaces.biome;

namespace MinecraftWorldsAPI.Models;

public record FeatureContext(Chunk Chunk, IBiomeSource BiomeSource, IRandom Random);
