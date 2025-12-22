using Microsoft.AspNetCore.Mvc;
using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Models.Enums;
using MinecraftWorldsAPI.Services.Noise;

namespace MinecraftWorldsAPI.Controllers;

/// Контроллер для тестирования генерации шума
[ApiController]
[Route("api/[controller]")]
[Tags("Noise Testing")]
public class NoiseTestController : ControllerBase
{
    private readonly IRandomFactory _randomFactory;

    public NoiseTestController(IRandomFactory randomFactory)
    {
        _randomFactory = randomFactory;
    }
    /// <summary>
    /// Тестирование 2D шума с визуализацией
    /// </summary>
    /// <param name="seed">Сид для генерации (по умолчанию: 12345)</param>
    /// <param name="width">Ширина карты (по умолчанию: 50)</param>
    /// <param name="height">Высота карты (по умолчанию: 30)</param>
    /// <param name="frequency">Частота шума (по умолчанию: 0.1)</param>
    /// <param name="octaves">Количество октав (по умолчанию: 4)</param>
    /// <param name="type">Тип генератора случайных чисел (по умолчанию: XorShift64)</param>
    /// <returns>Результат тестирования с визуализацией</returns>
    [HttpGet("test2d")]
    [ProducesResponseType(typeof(NoiseTestResult), StatusCodes.Status200OK)]
    public IActionResult Test2D(
        [FromQuery] long seed = 12345,
        [FromQuery] int width = 50,
        [FromQuery] int height = 30,
        [FromQuery] double frequency = 0.1,
        [FromQuery] int octaves = 4,
        [FromQuery] PrngType type = PrngType.XorShift64)
    {
        _randomFactory.Type = type;

        var noise2D = new PerlinNoise2D(_randomFactory, seed, frequency, 1.0, octaves, 2.0, 0.5);
        var result = new NoiseTestResult();

        var values = new List<List<double>>();
        double min = double.MaxValue;
        double max = double.MinValue;
        double sum = 0;

        // Сначала собираем все значения
        for (int y = 0; y < height; y++)
        {
            var row = new List<double>();
            for (int x = 0; x < width; x++)
            {
                double value = noise2D.Sample(x, y);
                row.Add(value);

                min = Math.Min(min, value);
                max = Math.Max(max, value);
                sum += value;
            }
            values.Add(row);
        }

        // Теперь создаем визуализацию с правильной нормализацией
        var visualization = new System.Text.StringBuilder();
        var chars = " .:-=+*#%@";
        double range = max - min;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double value = values[y][x];
                // Нормализуем значение от 0 до 1 для визуализации
                double normalized = range > 0.0001 ? (value - min) / range : 0.5;
                int charIndex = Math.Clamp((int)(normalized * (chars.Length - 1)), 0, chars.Length - 1);
                visualization.Append(chars[charIndex]);
            }
            visualization.AppendLine();
        }

        result.Visualization = visualization.ToString();
        result.Values = values;
        result.MinValue = min;
        result.MaxValue = max;
        result.AverageValue = sum / (width * height);

        // Проверка плавности - берем несколько точек и их соседей
        result.SamplePoints["center"] = noise2D.Sample(width / 2.0, height / 2.0);
        result.SamplePoints["center+0.1"] = noise2D.Sample(width / 2.0 + 0.1, height / 2.0);
        result.SamplePoints["center+0.5"] = noise2D.Sample(width / 2.0 + 0.5, height / 2.0);
        result.SamplePoints["center+1.0"] = noise2D.Sample(width / 2.0 + 1.0, height / 2.0);

        return Ok(result);
    }

    /// <summary>
    /// Тестирование 3D шума
    /// </summary>
    /// <param name="seed">Сид для генерации (по умолчанию: 12345)</param>
    /// <param name="size">Размер среза (по умолчанию: 10)</param>
    /// <param name="frequency">Частота шума (по умолчанию: 0.1)</param>
    /// <param name="octaves">Количество октав (по умолчанию: 4)</param>
    /// <param name="type">Тип генератора случайных чисел (по умолчанию: XorShift64)</param>
    /// <returns>Результат тестирования 3D шума</returns>
    [HttpGet("test3d")]
    [ProducesResponseType(typeof(NoiseTestResult), StatusCodes.Status200OK)]
    public IActionResult Test3D(
        [FromQuery] long seed = 12345,
        [FromQuery] int size = 10,
        [FromQuery] double frequency = 0.1,
        [FromQuery] int octaves = 4,
        [FromQuery] PrngType type = PrngType.XorShift64)
    {
        _randomFactory.Type = type;
        var noise3D = new PerlinNoise(_randomFactory, seed, frequency, 1.0, octaves, 2.0, 0.5);
        var result = new NoiseTestResult();

        var values = new List<List<double>>();
        double min = double.MaxValue;
        double max = double.MinValue;
        double sum = 0;

        // Генерируем срез 3D шума по Y=0
        for (int z = 0; z < size; z++)
        {
            var row = new List<double>();
            for (int x = 0; x < size; x++)
            {
                double value = noise3D.Sample(x, 0, z);
                row.Add(value);

                min = Math.Min(min, value);
                max = Math.Max(max, value);
                sum += value;
            }
            values.Add(row);
        }

        result.Values = values;
        result.MinValue = min;
        result.MaxValue = max;
        result.AverageValue = sum / (size * size);

        // Проверка плавности в 3D пространстве
        result.SamplePoints["origin"] = noise3D.Sample(0, 0, 0);
        result.SamplePoints["origin+0.1"] = noise3D.Sample(0.1, 0, 0);
        result.SamplePoints["origin+0.5"] = noise3D.Sample(0.5, 0, 0);
        result.SamplePoints["origin+1.0"] = noise3D.Sample(1.0, 0, 0);
        result.SamplePoints["y+1"] = noise3D.Sample(0, 1, 0);
        result.SamplePoints["z+1"] = noise3D.Sample(0, 0, 1);

        return Ok(result);
    }

    /// <summary>
    /// Тестирование плавности переходов
    /// </summary>
    /// <param name="seed">Сид для генерации (по умолчанию: 12345)</param>
    /// <param name="startX">Начальная координата X (по умолчанию: 0)</param>
    /// <param name="startY">Начальная координата Y (по умолчанию: 0)</param>
    /// <param name="steps">Количество шагов (по умолчанию: 20)</param>
    /// <param name="stepSize">Размер шага (по умолчанию: 0.1)</param>
    /// <param name="type">Тип генератора случайных чисел (по умолчанию: XorShift64)</param>
    /// <returns>Результат проверки плавности переходов</returns>
    [HttpGet("test-smoothness")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult TestSmoothness(
        [FromQuery] long seed = 12345,
        [FromQuery] double startX = 0,
        [FromQuery] double startY = 0,
        [FromQuery] int steps = 20,
        [FromQuery] double stepSize = 0.1,
        [FromQuery] PrngType type = PrngType.XorShift64)
    {
        _randomFactory.Type = type;
        var noise2D = new PerlinNoise2D(_randomFactory, seed, 0.1, 1.0, 4, 2.0, 0.5);

        var points = new List<Dictionary<string, double>>();
        double maxJump = 0;

        double prevValue = noise2D.Sample(startX, startY);

        for (int i = 0; i <= steps; i++)
        {
            double x = startX + i * stepSize;
            double value = noise2D.Sample(x, startY);
            double jump = Math.Abs(value - prevValue);
            maxJump = Math.Max(maxJump, jump);

            points.Add(new Dictionary<string, double>
            {
                ["x"] = x,
                ["value"] = value,
                ["jump"] = jump
            });

            prevValue = value;
        }

        return Ok(new
        {
            points,
            maxJump,
            averageJump = points.Skip(1).Average(p => p["jump"]),
            message = maxJump < 0.5 ? "Переходы плавные" : "Обнаружены резкие скачки"
        });
    }

    /// <summary>
    /// Тестирование реестра шумов
    /// </summary>
    /// <param name="seed">Сид для генерации (по умолчанию: 12345)</param>
    /// <param name="type">Тип генератора случайных чисел (по умолчанию: XorShift64)</param>
    /// <returns>Результат тестирования реестра</returns>
    [HttpGet("test-registry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult TestRegistry(
        [FromQuery] long seed = 12345,
        [FromQuery] PrngType type = PrngType.XorShift64)
    {
        _randomFactory.Type = type;
        var registry = new NoiseRegistry(_randomFactory, seed);

        // Регистрируем несколько шумов с разными настройками
        registry.RegisterNoise2D("terrain", new NoiseSettings(0.05, 1.0, 4, 2.0, 0.5));
        registry.RegisterNoise2D("detail", new NoiseSettings(0.2, 0.3, 2, 2.0, 0.5));
        registry.RegisterNoise("density", new NoiseSettings(0.1, 1.0, 3, 2.0, 0.5));

        var terrainNoise = registry.GetNoise2D("terrain");
        var detailNoise = registry.GetNoise2D("detail");
        var densityNoise = registry.GetNoise("density");

        return Ok(new
        {
            registeredNoises = new[] { "terrain", "detail", "density" },
            samples = new
            {
                terrain = terrainNoise.Sample(10, 10),
                detail = detailNoise.Sample(10, 10),
                density = densityNoise.Sample(10, 10, 10)
            },
            message = "Реестр шумов работает корректно"
        });
    }
}

