using MinecraftWorldsAPI.Services.Surface;

namespace MinecraftWorldsAPI.Services.Surface.Conditions;

/// <summary>
/// Условие проверки воды
/// Проверяет, находится ли текущая позиция над водой
/// </summary>
public class WaterCondition : ISurfaceCondition
{
    private readonly int _offset;
    private readonly int _surfaceDepthMultiplier;
    private readonly bool _addStoneDepth;

    public WaterCondition(int offset = 0, int surfaceDepthMultiplier = 0, bool addStoneDepth = false)
    {
        _offset = offset;
        _surfaceDepthMultiplier = surfaceDepthMultiplier;
        _addStoneDepth = addStoneDepth;
    }

    public bool Test(SurfaceContext context)
    {
        // Если над блоком нет воды, условие всегда выполняется
        if (!context.HasWaterAbove)
            return true;

        var adjustedOffset = _offset;
        adjustedOffset += context.SurfaceDepth * _surfaceDepthMultiplier;

        if (_addStoneDepth)
        {
            adjustedOffset += context.StoneDepthAbove;
        }

        return context.WaterHeight!.Value >= adjustedOffset;
    }
}

