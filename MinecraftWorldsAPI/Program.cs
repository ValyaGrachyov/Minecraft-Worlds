using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Services.PRNG;
using MinecraftWorldsAPI.Services.Random;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRandomFactory, LCGRandomFactory>();
builder.Services.AddSingleton<IRandomFactoryWithType, PrngFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/test", () => "Hello, Minecraft Worlds API!");

app.MapControllers();

app.Run();
