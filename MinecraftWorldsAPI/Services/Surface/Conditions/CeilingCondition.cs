using MinecraftWorldsAPI.Services.Surface;

namespace MinecraftWorldsAPI.Services.Surface.Conditions;

/// <summary>
/// Условие проверки потолка
/// Проверяет, является ли текущая позиция потолком
/// </summary>
public class CeilingCondition : ISurfaceCondition
{
    public bool Test(SurfaceContext context)
    {
        return context.IsCeiling;
    }
}

