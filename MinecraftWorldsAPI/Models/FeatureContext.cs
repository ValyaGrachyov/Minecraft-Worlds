using MinecraftWorldsAPI.Interfaces;

namespace MinecraftWorldsAPI.Models;

public record FeatureContext(Chunk Chunk, IBiomeSource BiomeSource, IRandom Random);
