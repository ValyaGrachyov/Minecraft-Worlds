using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Surface;

namespace MinecraftWorldsAPI.Services.Surface.Conditions;

/// <summary>
/// Условие проверки биома
/// Проверяет, находится ли текущий блок в указанном биоме
/// </summary>
public class BiomeCondition : ISurfaceCondition
{
    private readonly HashSet<Biome> _allowedBiomes;

    public BiomeCondition(params Biome[] biomes)
    {
        _allowedBiomes = new HashSet<Biome>(biomes);
    }

    public bool Test(SurfaceContext context)
    {
        return _allowedBiomes.Contains(context.Biome);
    }
}

