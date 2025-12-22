namespace MinecraftWorldsAPI.Models;

/// <summary>
/// Константы для имен шумов в реестре
/// Централизованное хранение имен для избежания магических строк
/// </summary>
public static class NoiseNames
{
    // Климатические шумы (для биомов)
    public const string Temperature = "temperature";
    public const string Humidity = "humidity";

    // Шумы для генерации ландшафта
    public const string Continents = "continents";      // Континентальность (океан/суша)
    public const string Erosion = "erosion";            // Эрозия (ровность местности)
    public const string Ridges = "ridges";              // Вершины и долины (weirdness)
    public const string Depth = "depth";                // Глубина (для сплайнов)
    public const string Density = "density";            // Основной 3D шум плотности

    // Шумы для генерации пещер (3D)
    public const string CaveCheese = "cave_cheese";     // Сырные пещеры (карманные области)
    public const string CaveSpaghetti = "cave_spaghetti"; // Спагетти-пещеры (длинные узкие)
    public const string CaveNoodle = "cave_noodle";     // Лапшичные пещеры (узкие извилистые)
    public const string CavePillars = "cave_pillars";   // Шумовые столбы (внутри камер)

    // Шумы для генерации поверхности
    public const string Surface = "surface";            // Шум поверхности (для вычисления surface depth)
    public const string SurfaceSecondary = "surface_secondary"; // Вторичный шум поверхности
}

