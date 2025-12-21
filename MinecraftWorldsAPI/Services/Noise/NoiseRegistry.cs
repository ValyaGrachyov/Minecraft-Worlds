using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Implementations;

/// Реестр шумов для хранения и получения шумов по имени
public class NoiseRegistry : INoiseRegistry
{
    private readonly Dictionary<string, INoise> _noises;
    private readonly Dictionary<string, INoise2D> _noises2D;
    private readonly long _seed;
    private readonly IRandomFactory _randomFactory;

    public NoiseRegistry(IRandomFactory randomFactory, long seed)
    {
        _randomFactory = randomFactory;
        _seed = seed;
        _noises = new Dictionary<string, INoise>();
        _noises2D = new Dictionary<string, INoise2D>();
    }

    /// Регистрирует 3D шум с заданными настройками
    public void RegisterNoise(string name, NoiseSettings? settings = null)
    {
        if (settings == null)
        {
            settings = new NoiseSettings(1.0, 1.0, 1, 2.0, 0.5);
        }

        var noise = new PerlinNoise(
            _randomFactory,
            _seed,
            settings.Frequency,
            settings.Amplitude,
            (int)settings.Octaves,
            settings.Lacunarity,
            settings.Persistence
        );

        _noises[name] = noise;
    }

    /// Регистрирует 2D шум с заданными настройками
    public void RegisterNoise2D(string name, NoiseSettings? settings = null)
    {
        if (settings == null)
        {
            settings = new NoiseSettings(1.0, 1.0, 1, 2.0, 0.5);
        }

        var noise = new PerlinNoise2D(
            _randomFactory,
            _seed,
            settings.Frequency,
            settings.Amplitude,
            (int)settings.Octaves,
            settings.Lacunarity,
            settings.Persistence
        );

        _noises2D[name] = noise;
    }

    /// Регистрирует готовый 3D шум
    public void RegisterNoise(string name, INoise noise)
    {
        _noises[name] = noise;
    }

    /// Регистрирует готовый 2D шум
    public void RegisterNoise2D(string name, INoise2D noise)
    {
        _noises2D[name] = noise;
    }

    public INoise GetNoise(string name)
    {
        if (!_noises.TryGetValue(name, out var noise))
        {
            throw new KeyNotFoundException($"Шум с именем '{name}' не найден в реестре. Используйте RegisterNoise для регистрации.");
        }

        return noise;
    }

    public INoise2D GetNoise2D(string name)
    {
        if (!_noises2D.TryGetValue(name, out var noise))
        {
            throw new KeyNotFoundException($"2D шум с именем '{name}' не найден в реестре. Используйте RegisterNoise2D для регистрации.");
        }

        return noise;
    }

    /// Проверяет, зарегистрирован ли шум с указанным именем
    public bool HasNoise(string name)
    {
        return _noises.ContainsKey(name);
    }

    /// Проверяет, зарегистрирован ли 2D шум с указанным именем
    public bool HasNoise2D(string name)
    {
        return _noises2D.ContainsKey(name);
    }
}
