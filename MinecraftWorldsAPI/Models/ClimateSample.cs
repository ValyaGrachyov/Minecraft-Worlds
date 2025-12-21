namespace MinecraftWorldsAPI.Models;

public readonly struct ClimateSample(double temperature, double humidity)
{
    public double Temperature { get; } = temperature; // -1..1 или 0..1
    public double Humidity { get; } = humidity; // -1..1 или 0..1
}