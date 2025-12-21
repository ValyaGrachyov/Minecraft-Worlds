using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models.Enums;

namespace MinecraftWorldsAPI.Services.Noise;

/// Реализация 2D шума Перлина для контроля высоты поверхности
public class PerlinNoise2D : INoise2D
{
    private readonly int[] _permutation;
    private readonly double _frequency;
    private readonly double _amplitude;
    private readonly int _octaves;
    private readonly double _lacunarity;
    private readonly double _persistence;

    // Стандартная таблица перестановок для Perlin noise
    private static readonly int[] PermutationTable = {
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
        140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
        247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
        57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
        74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
        60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
        65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
        200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
        52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
        207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
        119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
        218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
        81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
        184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
        222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };

    public PerlinNoise2D(IRandomFactory randomFactory, long seed, double frequency = 1.0, double amplitude = 1.0, int octaves = 1, double lacunarity = 2.0, double persistence = 0.5)
    {
        _frequency = frequency;
        _amplitude = amplitude;
        _octaves = octaves;
        _lacunarity = lacunarity;
        _persistence = persistence;

        // Создаем таблицу перестановок на основе seed
        _permutation = new int[512];
        var random = randomFactory.CreateRandom(seed, PrngType.XorShift64);
        
        // Копируем базовую таблицу
        for (int i = 0; i < 256; i++)
        {
            _permutation[i] = PermutationTable[i];
        }

        // Перемешиваем на основе seed
        for (int i = 0; i < 256; i++)
        {
            int j = random.NextInt(256);
            (_permutation[i], _permutation[j]) = (_permutation[j], _permutation[i]);
        }

        // Дублируем для упрощения вычислений
        for (int i = 0; i < 256; i++)
        {
            _permutation[256 + i] = _permutation[i];
        }
    }

    public double Sample(double x, double y)
    {
        x *= _frequency;
        y *= _frequency;

        double value = 0.0;
        double amplitude = _amplitude;
        double frequency = 1.0;

        for (int i = 0; i < _octaves; i++)
        {
            value += Noise2D(x * frequency, y * frequency) * amplitude;
            frequency *= _lacunarity;
            amplitude *= _persistence;
        }

        return value;
    }

    private double Noise2D(double x, double y)
    {
        // Определяем единичный квадрат, содержащий точку
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;

        // Находим относительные координаты точки внутри квадрата
        x -= Math.Floor(x);
        y -= Math.Floor(y);

        // Вычисляем функции затухания для каждой из переменных
        double u = Fade(x);
        double v = Fade(y);

        // Получаем хеши для 4 углов квадрата
        int A = _permutation[X] + Y;
        int AA = _permutation[A];
        int AB = _permutation[A + 1];
        int B = _permutation[X + 1] + Y;
        int BA = _permutation[B];
        int BB = _permutation[B + 1];

        // Интерполируем между 4 градиентами квадрата
        return Lerp(v,
            Lerp(u, Grad(_permutation[AA], x, y),
                Grad(_permutation[BA], x - 1, y)),
            Lerp(u, Grad(_permutation[AB], x, y - 1),
                Grad(_permutation[BB], x - 1, y - 1)));
    }

    private static double Fade(double t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static double Lerp(double t, double a, double b)
    {
        return a + t * (b - a);
    }

    private static double Grad(int hash, double x, double y)
    {
        int h = hash & 3;
        double u = h < 2 ? x : y;
        double v = h == 0 ? y : h == 1 ? -y : x;
        return u + v;
    }
}
