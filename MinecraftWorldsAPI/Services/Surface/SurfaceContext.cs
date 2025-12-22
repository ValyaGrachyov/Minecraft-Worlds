using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Surface;

/// <summary>
/// Контекст для вычисления условий поверхности
/// Содержит информацию о позиции, биоме, глубине поверхности и других параметрах
/// </summary>
public class SurfaceContext
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    public Biome Biome { get; }
    
    /// <summary>
    /// Глубина поверхности (surface depth) - целое число, вычисляется для каждого столбца
    /// Используется для определения расстояния до поверхности
    /// </summary>
    public int SurfaceDepth { get; set; }
    
    /// <summary>
    /// Вторичная глубина поверхности (surface secondary depth) - значение от -1 до 1
    /// </summary>
    public double SurfaceSecondaryDepth { get; set; }
    
    /// <summary>
    /// Расстояние до поверхности сверху (stoneDepthAbove)
    /// </summary>
    public int StoneDepthAbove { get; set; }
    
    /// <summary>
    /// Расстояние до поверхности снизу (stoneDepthBelow)
    /// </summary>
    public int StoneDepthBelow { get; set; }
    
    /// <summary>
    /// Высота воды над блоком (waterHeight), если есть вода
    /// </summary>
    public int? WaterHeight { get; set; }
    
    /// <summary>
    /// Является ли блок потолком (ceiling)
    /// </summary>
    public bool IsCeiling { get; set; }
    
    /// <summary>
    /// Есть ли вода над блоком
    /// </summary>
    public bool HasWaterAbove => WaterHeight.HasValue;

    public SurfaceContext(int x, int y, int z, Biome biome)
    {
        X = x;
        Y = y;
        Z = z;
        Biome = biome;
    }
}

