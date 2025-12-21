using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Services.Biome;
using MinecraftWorldsAPI.Services.Noise;
using MinecraftWorldsAPI.Services.Random;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRandomFactory, LCGRandomFactory>();
builder.Services.AddScoped<IClimateSampler>(sp =>
{
    var rf = sp.GetRequiredService<IRandomFactory>();

    var temperatureNoise = new PerlinNoise2D(
        rf,
        seed: 1001,
        frequency: 1,
        amplitude: 1.0,
        octaves: 4,
        lacunarity: 2.0,
        persistence: 0.5
    );

    var humidityNoise = new PerlinNoise2D(
        rf,
        seed: 2002,
        frequency: 0.1,
        amplitude: 1.0,
        octaves: 4,
        lacunarity: 2.0,
        persistence: 0.5
    );

    return new ClimateSampler(
        temperatureNoise,
        humidityNoise
    );
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
