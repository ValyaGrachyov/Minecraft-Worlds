using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Surface;

/// <summary>
/// Интерфейс для правил поверхности
/// Определяет, какой блок должен быть размещен в данной позиции
/// </summary>
public interface ISurfaceRule
{
    /// <summary>
    /// Применяет правило к контексту и возвращает блок, который должен быть размещен
    /// Возвращает null, если правило не применяется
    /// </summary>
    Block? Apply(SurfaceContext context);
}

