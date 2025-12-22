namespace MinecraftWorldsAPI.Models;

/// <summary>
/// Значения по умолчанию для настройки шумов
/// Централизованное хранение настроек для избежания дублирования
/// </summary>
public static class NoiseDefaults
{
    // Настройки по умолчанию для базового шума
    public static readonly NoiseSettings Default = new(
        Frequency: 1.0,
        Amplitude: 1.0,
        Octaves: 1,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    // Климатические шумы (для биомов)
    public static readonly NoiseSettings Temperature = new(
        Frequency: 1.0,
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    public static readonly NoiseSettings Humidity = new(
        Frequency: 0.1,
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    // Шумы для генерации ландшафта (2D для сплайнов)
    public static readonly NoiseSettings Continents = new(
        Frequency: 0.05,      // Низкая частота для больших континентов
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    public static readonly NoiseSettings Erosion = new(
        Frequency: 0.1,
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    public static readonly NoiseSettings Ridges = new(
        Frequency: 0.15,
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    public static readonly NoiseSettings Depth = new(
        Frequency: 0.1,
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    // Основной 3D шум для плотности
    public static readonly NoiseSettings Density = new(
        Frequency: 0.1,       // Низкая частота для плавного рельефа
        Amplitude: 1.0,
        Octaves: 6,          // Больше октав для детализации
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    // Seeds для климатических шумов
    public const long TemperatureSeed = 1001;
    public const long HumiditySeed = 2002;

    // Шумы для генерации пещер (3D)
    public static readonly NoiseSettings CaveCheese = new(
        Frequency: 0.15,      // Частота генерации сырных пещер
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    public static readonly NoiseSettings CaveSpaghetti = new(
        Frequency: 0.2,      // Частота генерации спагетти-пещер
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    public static readonly NoiseSettings CaveNoodle = new(
        Frequency: 0.25,     // Частота генерации лапшичных пещер (выше = чаще)
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    public static readonly NoiseSettings CavePillars = new(
        Frequency: 0.1,      // Частота генерации столбов
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    // Seeds для terrain шумов
    public const long ContinentsSeed = 3003;
    public const long ErosionSeed = 4004;
    public const long RidgesSeed = 5005;
    public const long DepthSeed = 6006;
    public const long DensitySeed = 7007;

    // Seeds для пещер
    public const long CaveCheeseSeed = 8008;
    public const long CaveSpaghettiSeed = 9009;
    public const long CaveNoodleSeed = 1010;
    public const long CavePillarsSeed = 1111;

    // Шумы для генерации поверхности (2D)
    public static readonly NoiseSettings Surface = new(
        Frequency: 0.1,
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    public static readonly NoiseSettings SurfaceSecondary = new(
        Frequency: 0.1,
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    // Seeds для поверхности
    public const long SurfaceSeed = 1212;
    public const long SurfaceSecondarySeed = 1313;

    // Параметры пещер (пороговые значения)
    public const double CaveCheeseHollowness = 0.3;      // Порог пустотелости для сырных пещер (чем выше, тем больше пещеры)
    public const double CaveSpaghettiThickness = 0.15;  // Порог толщины для спагетти-пещер
    public const double CaveNoodleThickness = 0.1;      // Порог толщины для лапшичных пещер (уже чем спагетти)
    public const double CavePillarsThickness = 0.2;     // Порог толщины для столбов
    public const double CavePillarsFrequency = 0.05;    // Частота генерации столбов
}

