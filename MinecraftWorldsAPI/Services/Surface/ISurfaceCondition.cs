namespace MinecraftWorldsAPI.Services.Surface;

/// <summary>
/// Интерфейс для условий поверхности
/// Определяет, выполняется ли условие для данного контекста
/// </summary>
public interface ISurfaceCondition
{
    /// <summary>
    /// Проверяет, выполняется ли условие для данного контекста
    /// </summary>
    bool Test(SurfaceContext context);
}

