namespace MinecraftWorldsAPI.Models;

/// Результат тестирования шума
public class NoiseTestResult
{
    public string Visualization { get; set; } = string.Empty;
    public List<List<double>> Values { get; set; } = new();
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double AverageValue { get; set; }
    public Dictionary<string, double> SamplePoints { get; set; } = new();
}