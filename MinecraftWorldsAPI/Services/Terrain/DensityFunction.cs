using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Terrain;

/// <summary>
/// Вычисляет плотность блоков для генерации рельефа
/// </summary>
public class DensityFunction(INoiseRegistry noiseRegistry) : IDensityFunction
{
    private const double TerrainScale = 0.01;
    private const double HeightScale = 0.02;
    
    public double ComputeDensity(int x, int y, int z)
    {
        // Получаем шум для высоты рельефа
        var heightNoise = noiseRegistry.GetNoise2D("terrain_height");
        var baseHeight = heightNoise.Sample(x * TerrainScale, z * TerrainScale);
        
        // Нормализуем высоту (примерно от 50 до 100 блоков)
        var targetHeight = 64 + baseHeight * 30;
        
        // Получаем 3D шум для детализации
        var detailNoise = noiseRegistry.GetNoise("terrain_detail");
        var detail = detailNoise.Sample(x * HeightScale, y * HeightScale, z * HeightScale) * 5;
        
        // Вычисляем плотность: чем выше y относительно целевой высоты, тем меньше плотность
        var density = targetHeight - y + detail;
        
        return density;
    }
}
