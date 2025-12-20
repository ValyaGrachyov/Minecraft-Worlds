namespace MinecraftWorldsAPI.Models;

public readonly struct ClimateSample
{
    public double Temperature { get; }
    public double Humidity { get; }

    public ClimateSample(double temperature, double humidity)
    {
        Temperature = temperature; // -1..1 или 0..1
        Humidity = humidity;       // -1..1 или 0..1
    }
}