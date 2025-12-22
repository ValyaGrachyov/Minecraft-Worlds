using System.Text.Json.Serialization;
using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Biomes;
using MinecraftWorldsAPI.Services.Caves;
using MinecraftWorldsAPI.Services.Fluids;
using MinecraftWorldsAPI.Services.Noise;
using MinecraftWorldsAPI.Services.PRNG;
using MinecraftWorldsAPI.Services.Surface;
using MinecraftWorldsAPI.Services.Terrain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRandomFactory, PrngFactory>();

builder.Services.AddScoped<IBiomeSource, BiomeSource>();

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

    // Регистрируем 2D шумы для terrain (continents, erosion, ridges, depth)
    var continentsNoise = new PerlinNoise2D(
        randomFactory,
        seed: NoiseDefaults.ContinentsSeed,
        frequency: NoiseDefaults.Continents.Frequency,
        amplitude: NoiseDefaults.Continents.Amplitude,
        octaves: (int)NoiseDefaults.Continents.Octaves,
        lacunarity: NoiseDefaults.Continents.Lacunarity,
        persistence: NoiseDefaults.Continents.Persistence
    );

    var erosionNoise = new PerlinNoise2D(
        randomFactory,
        seed: NoiseDefaults.ErosionSeed,
        frequency: NoiseDefaults.Erosion.Frequency,
        amplitude: NoiseDefaults.Erosion.Amplitude,
        octaves: (int)NoiseDefaults.Erosion.Octaves,
        lacunarity: NoiseDefaults.Erosion.Lacunarity,
        persistence: NoiseDefaults.Erosion.Persistence
    );

    var ridgesNoise = new PerlinNoise2D(
        randomFactory,
        seed: NoiseDefaults.RidgesSeed,
        frequency: NoiseDefaults.Ridges.Frequency,
        amplitude: NoiseDefaults.Ridges.Amplitude,
        octaves: (int)NoiseDefaults.Ridges.Octaves,
        lacunarity: NoiseDefaults.Ridges.Lacunarity,
        persistence: NoiseDefaults.Ridges.Persistence
    );

    var depthNoise = new PerlinNoise2D(
        randomFactory,
        seed: NoiseDefaults.DepthSeed,
        frequency: NoiseDefaults.Depth.Frequency,
        amplitude: NoiseDefaults.Depth.Amplitude,
        octaves: (int)NoiseDefaults.Depth.Octaves,
        lacunarity: NoiseDefaults.Depth.Lacunarity,
        persistence: NoiseDefaults.Depth.Persistence
    );

    registry.RegisterNoise2D(NoiseNames.Continents, continentsNoise);
    registry.RegisterNoise2D(NoiseNames.Erosion, erosionNoise);
    registry.RegisterNoise2D(NoiseNames.Ridges, ridgesNoise);
    registry.RegisterNoise2D(NoiseNames.Depth, depthNoise);

    // Регистрируем 3D шум для плотности
    var densityNoise = new PerlinNoise(
        randomFactory,
        seed: NoiseDefaults.DensitySeed,
        frequency: NoiseDefaults.Density.Frequency,
        amplitude: NoiseDefaults.Density.Amplitude,
        octaves: (int)NoiseDefaults.Density.Octaves,
        lacunarity: NoiseDefaults.Density.Lacunarity,
        persistence: NoiseDefaults.Density.Persistence
    );

    registry.RegisterNoise(NoiseNames.Density, densityNoise);

    // Регистрируем 3D шумы для пещер
    var cheeseNoise = new PerlinNoise(
        randomFactory,
        seed: NoiseDefaults.CaveCheeseSeed,
        frequency: NoiseDefaults.CaveCheese.Frequency,
        amplitude: NoiseDefaults.CaveCheese.Amplitude,
        octaves: (int)NoiseDefaults.CaveCheese.Octaves,
        lacunarity: NoiseDefaults.CaveCheese.Lacunarity,
        persistence: NoiseDefaults.CaveCheese.Persistence
    );

    var spaghettiNoise = new PerlinNoise(
        randomFactory,
        seed: NoiseDefaults.CaveSpaghettiSeed,
        frequency: NoiseDefaults.CaveSpaghetti.Frequency,
        amplitude: NoiseDefaults.CaveSpaghetti.Amplitude,
        octaves: (int)NoiseDefaults.CaveSpaghetti.Octaves,
        lacunarity: NoiseDefaults.CaveSpaghetti.Lacunarity,
        persistence: NoiseDefaults.CaveSpaghetti.Persistence
    );

    var noodleNoise = new PerlinNoise(
        randomFactory,
        seed: NoiseDefaults.CaveNoodleSeed,
        frequency: NoiseDefaults.CaveNoodle.Frequency,
        amplitude: NoiseDefaults.CaveNoodle.Amplitude,
        octaves: (int)NoiseDefaults.CaveNoodle.Octaves,
        lacunarity: NoiseDefaults.CaveNoodle.Lacunarity,
        persistence: NoiseDefaults.CaveNoodle.Persistence
    );

    var pillarsNoise = new PerlinNoise(
        randomFactory,
        seed: NoiseDefaults.CavePillarsSeed,
        frequency: NoiseDefaults.CavePillars.Frequency,
        amplitude: NoiseDefaults.CavePillars.Amplitude,
        octaves: (int)NoiseDefaults.CavePillars.Octaves,
        lacunarity: NoiseDefaults.CavePillars.Lacunarity,
        persistence: NoiseDefaults.CavePillars.Persistence
    );

    registry.RegisterNoise(NoiseNames.CaveCheese, cheeseNoise);
    registry.RegisterNoise(NoiseNames.CaveSpaghetti, spaghettiNoise);
    registry.RegisterNoise(NoiseNames.CaveNoodle, noodleNoise);
    registry.RegisterNoise(NoiseNames.CavePillars, pillarsNoise);

    // Регистрируем 2D шумы для поверхности
    var surfaceNoise = new PerlinNoise2D(
        randomFactory,
        seed: NoiseDefaults.SurfaceSeed,
        frequency: NoiseDefaults.Surface.Frequency,
        amplitude: NoiseDefaults.Surface.Amplitude,
        octaves: (int)NoiseDefaults.Surface.Octaves,
        lacunarity: NoiseDefaults.Surface.Lacunarity,
        persistence: NoiseDefaults.Surface.Persistence
    );

    var surfaceSecondaryNoise = new PerlinNoise2D(
        randomFactory,
        seed: NoiseDefaults.SurfaceSecondarySeed,
        frequency: NoiseDefaults.SurfaceSecondary.Frequency,
        amplitude: NoiseDefaults.SurfaceSecondary.Amplitude,
        octaves: (int)NoiseDefaults.SurfaceSecondary.Octaves,
        lacunarity: NoiseDefaults.SurfaceSecondary.Lacunarity,
        persistence: NoiseDefaults.SurfaceSecondary.Persistence
    );

    registry.RegisterNoise2D(NoiseNames.Surface, surfaceNoise);
    registry.RegisterNoise2D(NoiseNames.SurfaceSecondary, surfaceSecondaryNoise);

    return registry;
});

builder.Services.AddScoped<IClimateSampler>(sp =>
{
    var noiseRegistry = sp.GetRequiredService<INoiseRegistry>();
    return new ClimateSampler(noiseRegistry);
});


// Регистрация сервисов для генерации ландшафта
builder.Services.AddScoped<IDensityFunction>(sp =>
{
    var noiseRegistry = sp.GetRequiredService<INoiseRegistry>();
    var biomeSource = sp.GetRequiredService<IBiomeSource>();
    return new DensityFunction(noiseRegistry, biomeSource);
});

builder.Services.AddScoped<ITerrainGenerator, TerrainGenerator>();

// Регистрация генератора пещер
builder.Services.AddScoped<ICaveGenerator, CaveGenerator>();

// Регистрация заполнителя жидкостями
builder.Services.AddScoped<IFluidFiller, FluidFiller>();

// Регистрация генератора поверхности
builder.Services.AddScoped<ISurfaceBuilder>(sp =>
{
    var noiseRegistry = sp.GetRequiredService<INoiseRegistry>();
    var randomFactory = sp.GetRequiredService<IRandomFactory>();
    return new SurfaceBuilder(noiseRegistry, randomFactory);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapSwagger();
}

app.MapGet("/test", () => "Hello, Minecraft Worlds API!");

app.MapControllers();

app.Run();
