using MinecraftWorldsAPI.Services.Surface;

namespace MinecraftWorldsAPI.Services.Surface.Conditions;

/// <summary>
/// Условие проверки глубины камня
/// Проверяет, находится ли текущая позиция в пределах указанного расстояния от поверхности
/// </summary>
public class StoneDepthCondition : ISurfaceCondition
{
    private readonly SurfaceType _surfaceType;
    private readonly int _offset;
    private readonly bool _addSurfaceDepth;
    private readonly int _secondaryDepthRange;

    public enum SurfaceType
    {
        Floor,   // Расстояние до поверхности сверху
        Ceiling  // Расстояние до поверхности снизу
    }

    public StoneDepthCondition(SurfaceType surfaceType, int offset, bool addSurfaceDepth = false, int secondaryDepthRange = 0)
    {
        _surfaceType = surfaceType;
        _offset = offset;
        _addSurfaceDepth = addSurfaceDepth;
        _secondaryDepthRange = secondaryDepthRange;
    }

    public bool Test(SurfaceContext context)
    {
        var depth = _surfaceType == SurfaceType.Floor 
            ? context.StoneDepthAbove 
            : context.StoneDepthBelow;

        var adjustedOffset = _offset;
        
        if (_addSurfaceDepth)
        {
            adjustedOffset += context.SurfaceDepth;
        }

        if (_secondaryDepthRange > 0)
        {
            // map(surface_secondary(X,0,Z), -1, 1, 0, secondary_depth_range)
            var mappedSecondary = (context.SurfaceSecondaryDepth + 1.0) * 0.5 * _secondaryDepthRange;
            adjustedOffset += (int)mappedSecondary;
        }

        return depth <= adjustedOffset;
    }
}

