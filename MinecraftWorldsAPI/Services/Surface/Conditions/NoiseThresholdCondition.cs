using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Surface;

namespace MinecraftWorldsAPI.Services.Surface.Conditions;

/// <summary>
/// Условие проверки порога шума
/// Вычисляет значение шума текущего столбца и проверяет, находится ли оно между min и max
/// </summary>
public class NoiseThresholdCondition : ISurfaceCondition
{
    private readonly INoise2D _noise;
    private readonly double _minThreshold;
    private readonly double _maxThreshold;
    private readonly double _scale;

    public NoiseThresholdCondition(INoise2D noise, double minThreshold, double maxThreshold, double scale = 0.05)
    {
        _noise = noise;
        _minThreshold = minThreshold;
        _maxThreshold = maxThreshold;
        _scale = scale;
    }

    public bool Test(SurfaceContext context)
    {
        var noiseValue = _noise.Sample(context.X * _scale, context.Z * _scale);
        return noiseValue >= _minThreshold && noiseValue <= _maxThreshold;
    }
}

