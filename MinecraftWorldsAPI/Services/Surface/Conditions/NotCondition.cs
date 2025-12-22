using MinecraftWorldsAPI.Services.Surface;

namespace MinecraftWorldsAPI.Services.Surface.Conditions;

/// <summary>
/// Условие инверсии
/// Инвертирует результат другого условия
/// </summary>
public class NotCondition : ISurfaceCondition
{
    private readonly ISurfaceCondition _condition;

    public NotCondition(ISurfaceCondition condition)
    {
        _condition = condition;
    }

    public bool Test(SurfaceContext context)
    {
        return !_condition.Test(context);
    }
}

