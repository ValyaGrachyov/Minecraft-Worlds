namespace MinecraftWorldsAPI.Models;

// Значения по умолчанию для настройки шумов
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

    // Настройки для температурного шума
    public static readonly NoiseSettings Temperature = new(
        Frequency: 1.0,
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    // Настройки для шума влажности
    public static readonly NoiseSettings Humidity = new(
        Frequency: 0.1,
        Amplitude: 1.0,
        Octaves: 4,
        Lacunarity: 2.0,
        Persistence: 0.5
    );

    // Seed для температурного шума
    public const long TemperatureSeed = 1001;

    // Seed для шума влажности
    public const long HumiditySeed = 2002;
}

