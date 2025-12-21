using Microsoft.AspNetCore.Mvc;
using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Biome;

namespace MinecraftWorldsAPI.Controllers;

/// Контроллер для тестирования биомов и климата
[ApiController]
[Route("api/[controller]")]
[Tags("Biome Testing")]
public sealed class BiomeTestController(
    IBiomeSource biomeSource,
    IClimateSampler climateSampler)
    : ControllerBase
{
    /// <summary>
    /// 2D-карта биомов с ASCII-визуализацией
    /// </summary>
    [HttpGet("map")]
    public IActionResult TestBiomeMap(
        [FromQuery] int width = 50,
        [FromQuery] int height = 30,
        [FromQuery] int y = 0)
    {
        var visualization = new System.Text.StringBuilder();
        var stats = new Dictionary<Biome, int>();

        for (var z = 0; z < height; z++)
        {
            for (var x = 0; x < width; x++)
            {
                var biome = biomeSource.GetBiome(x, y, z);

                if (!stats.ContainsKey(biome))
                    stats[biome] = 0;

                stats[biome]++;

                visualization.Append(BiomeChar(biome));
            }

            visualization.AppendLine();
        }

        return Ok(new
        {
            visualization = visualization.ToString(),
            biomeStats = stats,
            message = "Карта биомов сгенерирована",
            legend = new Dictionary<string, string>
            {
                ["D"] = "Desert",
                ["."] = "Plains",
                ["F"] = "Forest",
                ["^"] = "ExtremeHills",
                ["~"] = "Ocean"
            },
        });
    }

    /// <summary>
    /// Тест климата (температура / влажность)
    /// </summary>
    [HttpGet("climate")]
    public IActionResult TestClimate(
        [FromQuery] int width = 30,
        [FromQuery] int height = 20,
        [FromQuery] int y = 0)
    {
        var temperatures = new List<List<double>>();
        var humidities = new List<List<double>>();

        double minT = 1, maxT = 0;
        double minH = 1, maxH = 0;

        for (int z = 0; z < height; z++)
        {
            var tRow = new List<double>();
            var hRow = new List<double>();

            for (int x = 0; x < width; x++)
            {
                var climate = climateSampler.Sample(x, y, z);

                tRow.Add(climate.Temperature);
                hRow.Add(climate.Humidity);

                minT = Math.Min(minT, climate.Temperature);
                maxT = Math.Max(maxT, climate.Temperature);

                minH = Math.Min(minH, climate.Humidity);
                maxH = Math.Max(maxH, climate.Humidity);
            }

            temperatures.Add(tRow);
            humidities.Add(hRow);
        }

        return Ok(new
        {
            temperature = new
            {
                values = temperatures,
                min = minT,
                max = maxT
            },
            humidity = new
            {
                values = humidities,
                min = minH,
                max = maxH
            }
        });
    }
    
    /// <summary>
    /// Точечные сэмплы биома и климата
    /// </summary>
    [HttpGet("sample")]
    public IActionResult TestSample(
        [FromQuery] int x = 10,
        [FromQuery] int y = 0,
        [FromQuery] int z = 10)
    {
        var biome = biomeSource.GetBiome(x, y, z);
        var climate = climateSampler.Sample(x, y, z);

        return Ok(new
        {
            position = new { x, y, z },
            biome = biome.ToString(),
            temperature = climate.Temperature,
            humidity = climate.Humidity
        });
    }

    private static char BiomeChar(Biome biome) => biome switch
    {
        Biome.Desert => 'D',
        Biome.Plains => '.',
        Biome.Forest => 'F',
        Biome.ExtremeHills => '^',
        Biome.Ocean => '~',
        _ => '?'
    };
}