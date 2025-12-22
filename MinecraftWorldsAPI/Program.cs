using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Biome;
using MinecraftWorldsAPI.Services.Noise;
using MinecraftWorldsAPI.Services.Random;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRandomFactory, LCGRandomFactory>();

builder.Services.AddScoped<INoiseRegistry>(sp =>
{
    var randomFactory = sp.GetRequiredService<IRandomFactory>();
    var registry = new NoiseRegistry(randomFactory, seed: 0); // Базовый seed, отдельные seeds для каждого шума

    // Регистрируем шумы с разными seeds через готовые экземпляры
    var temperatureNoise = new PerlinNoise2D(
        randomFactory,
        seed: NoiseDefaults.TemperatureSeed,
        frequency: NoiseDefaults.Temperature.Frequency,
        amplitude: NoiseDefaults.Temperature.Amplitude,
        octaves: (int)NoiseDefaults.Temperature.Octaves,
        lacunarity: NoiseDefaults.Temperature.Lacunarity,
        persistence: NoiseDefaults.Temperature.Persistence
    );

    var humidityNoise = new PerlinNoise2D(
        randomFactory,
        seed: NoiseDefaults.HumiditySeed,
        frequency: NoiseDefaults.Humidity.Frequency,
        amplitude: NoiseDefaults.Humidity.Amplitude,
        octaves: (int)NoiseDefaults.Humidity.Octaves,
        lacunarity: NoiseDefaults.Humidity.Lacunarity,
        persistence: NoiseDefaults.Humidity.Persistence
    );

    registry.RegisterNoise2D(NoiseNames.Temperature, temperatureNoise);
    registry.RegisterNoise2D(NoiseNames.Humidity, humidityNoise);

    return registry;
});

builder.Services.AddScoped<IClimateSampler>(sp =>
{
    var noiseRegistry = sp.GetRequiredService<INoiseRegistry>();
    return new ClimateSampler(noiseRegistry);
});

builder.Services.AddScoped<IBiomeSource, BiomeSource>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/test", () => "Hello, Minecraft Worlds API!");

app.MapControllers();

app.Run();
