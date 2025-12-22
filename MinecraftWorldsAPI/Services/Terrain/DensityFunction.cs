using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Terrain;

/// <summary>
/// Реализация функции плотности для генерации ландшафта
/// Основана на алгоритме Minecraft: использует y_clamped_gradient, 3D шум и параметры биома
/// </summary>
public class DensityFunction(INoiseRegistry noiseRegistry, IBiomeSource biomeSource, int minY = 0, int maxY = 255)
    : IDensityFunction
{
    // Параметры для y_clamped_gradient
    private const double FromValue = 1.0;  // Плотность на дне
    private const double ToValue = -3.0;    // Плотность на верху

    // Масштабы для шумов
    private const double DensityXZScale = 0.1;  // Масштаб по X/Z для 3D шума
    private const double DensityYScale = 0.1;   // Масштаб по Y для 3D шума
    private const double TerrainXZScale = 0.05;   // Масштаб для 2D шумов (continents, erosion, ridges)

    /// <summary>
    /// Вычисляет плотность для блока в позиции (x, y, z)
    /// Плотность > 0 означает твердый блок, <= 0 - воздух
    /// </summary>
    public double ComputeDensity(int x, int y, int z)
    {
        // 1. Базовый y_clamped_gradient - создает градиент от дна к верху
        var baseDensity = ComputeYClampedGradient(y);

        // 2. Получаем параметры биома для этой позиции
        var biome = biomeSource.GetBiome(x, y, z);
        var biomeParams = GetBiomeParameters(biome);

        // 3. Вычисляем 2D шумы для сплайнов (continents, erosion, ridges)
        var continents = noiseRegistry.GetNoise2D(NoiseNames.Continents)
            .Sample(x * TerrainXZScale, z * TerrainXZScale);
        
        var erosion = noiseRegistry.GetNoise2D(NoiseNames.Erosion)
            .Sample(x * TerrainXZScale, z * TerrainXZScale);
        
        var ridges = noiseRegistry.GetNoise2D(NoiseNames.Ridges)
            .Sample(x * TerrainXZScale, z * TerrainXZScale);

        // 4. Применяем влияние параметров на базовую высоту
        // Континентальность: чем выше, тем выше рельеф (различает океан/сушу)
        var heightOffset = continents * biomeParams.ContinentsMultiplier;
        
        // Эрозия: чем выше, тем ниже и ровнее рельеф
        var erosionEffect = erosion * biomeParams.ErosionMultiplier;
        
        // Вершины и долины: создает четкие вершины и долины
        var ridgesEffect = ridges * biomeParams.RidgesMultiplier;

        // 5. Вычисляем смещение высоты на основе параметров
        var heightShift = heightOffset - erosionEffect + ridgesEffect;

        // 6. Применяем 3D шум для создания объемных форм (overhangs, пещеры будут позже)
        var densityNoise = noiseRegistry.GetNoise(NoiseNames.Density)
            .Sample(x * DensityXZScale, y * DensityYScale, z * DensityXZScale);

        // 7. Комбинируем все компоненты
        // Базовый градиент + смещение высоты + 3D шум
        var finalDensity = baseDensity + heightShift * biomeParams.HeightMultiplier + densityNoise * biomeParams.DensityMultiplier;

        return finalDensity;
    }

    /// <summary>
    /// Вычисляет y_clamped_gradient: линейно интерполирует плотность от from_y к to_y
    /// </summary>
    private double ComputeYClampedGradient(int y)
    {
        // Ограничиваем Y между from_y и to_y
        var clampedY = Math.Clamp(y, minY, maxY);
        
        // Линейная интерполяция
        var t = (double)(clampedY - minY) / (maxY - minY);
        return FromValue + (ToValue - FromValue) * t;
    }

    /// <summary>
    /// Получает параметры генерации ландшафта для конкретного биома
    /// </summary>
    private static BiomeTerrainParameters GetBiomeParameters(Biome biome)
    {
        return biome switch
        {
            Biome.Ocean => new BiomeTerrainParameters(
                HeightMultiplier: 0.3,      // Низкий рельеф
                DensityMultiplier: 0.2,     // Плавный
                ContinentsMultiplier: -0.5, // Океан - низкая континентальность
                ErosionMultiplier: 0.8,      // Высокая эрозия (ровно)
                RidgesMultiplier: 0.1       // Минимальные вершины/долины
            ),
            Biome.Plains => new BiomeTerrainParameters(
                HeightMultiplier: 0.5,
                DensityMultiplier: 0.3,
                ContinentsMultiplier: 0.3,
                ErosionMultiplier: 0.7,     // Высокая эрозия для ровности
                RidgesMultiplier: 0.2
            ),
            Biome.Desert => new BiomeTerrainParameters(
                HeightMultiplier: 0.4,
                DensityMultiplier: 0.25,
                ContinentsMultiplier: 0.4,
                ErosionMultiplier: 0.6,
                RidgesMultiplier: 0.3       // Немного дюн
            ),
            Biome.Forest => new BiomeTerrainParameters(
                HeightMultiplier: 0.6,
                DensityMultiplier: 0.4,
                ContinentsMultiplier: 0.5,
                ErosionMultiplier: 0.4,
                RidgesMultiplier: 0.4
            ),
            Biome.Mountains => new BiomeTerrainParameters(
                HeightMultiplier: 1.5,     // Высокий рельеф
                DensityMultiplier: 0.8,     // Больше вариаций
                ContinentsMultiplier: 0.8,  // Высокая континентальность
                ErosionMultiplier: 0.2,    // Низкая эрозия (сохраняет высоту)
                RidgesMultiplier: 1.2       // Сильные вершины и долины
            ),
            _ => new BiomeTerrainParameters(
                HeightMultiplier: 0.5,
                DensityMultiplier: 0.3,
                ContinentsMultiplier: 0.3,
                ErosionMultiplier: 0.5,
                RidgesMultiplier: 0.3
            )
        };
    }

    /// <summary>
    /// Параметры генерации ландшафта для биома
    /// </summary>
    private sealed record BiomeTerrainParameters(
        double HeightMultiplier,      // Множитель для смещения высоты
        double DensityMultiplier,     // Множитель для 3D шума плотности
        double ContinentsMultiplier,  // Влияние континентальности
        double ErosionMultiplier,     // Влияние эрозии
        double RidgesMultiplier       // Влияние вершин/долин
    );
}

