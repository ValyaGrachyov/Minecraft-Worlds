using MinecraftWorldsAPI.Interfaces;

namespace MinecraftWorldsAPI.Implementations;

/// Реализация 3D шума Перлина для генерации плотности блоков
public class PerlinNoise : INoise
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

    public PerlinNoise(long seed, double frequency = 1.0, double amplitude = 1.0, int octaves = 1, double lacunarity = 2.0, double persistence = 0.5)
    {
        _frequency = frequency;
        _amplitude = amplitude;
        _octaves = octaves;
        _lacunarity = lacunarity;
        _persistence = persistence;

        // Создаем таблицу перестановок на основе seed
        _permutation = new int[512];
        var random = new System.Random((int)(seed & 0x7FFFFFFF));
        
        // Копируем базовую таблицу
        for (int i = 0; i < 256; i++)
        {
            _permutation[i] = PermutationTable[i];
        }

        // Перемешиваем на основе seed
        for (int i = 0; i < 256; i++)
        {
            int j = random.Next(256);
            (_permutation[i], _permutation[j]) = (_permutation[j], _permutation[i]);
        }

        // Дублируем для упрощения вычислений
        for (int i = 0; i < 256; i++)
        {
            _permutation[256 + i] = _permutation[i];
        }
    }

    public double Sample(double x, double y, double z)
    {
        x *= _frequency;
        y *= _frequency;
        z *= _frequency;

        double value = 0.0;
        double amplitude = _amplitude;
        double frequency = 1.0;

        for (int i = 0; i < _octaves; i++)
        {
            value += Noise3D(x * frequency, y * frequency, z * frequency) * amplitude;
            frequency *= _lacunarity;
            amplitude *= _persistence;
        }

        return value;
    }

    private double Noise3D(double x, double y, double z)
    {
        // Определяем единичный куб, содержащий точку
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;
        int Z = (int)Math.Floor(z) & 255;

        // Находим относительные координаты точки внутри куба
        x -= Math.Floor(x);
        y -= Math.Floor(y);
        z -= Math.Floor(z);

        // Вычисляем функции затухания для каждой из переменных
        double u = Fade(x);
        double v = Fade(y);
        double w = Fade(z);

        // Получаем хеши для 8 углов куба
        int A = _permutation[X] + Y;
        int AA = _permutation[A] + Z;
        int AB = _permutation[A + 1] + Z;
        int B = _permutation[X + 1] + Y;
        int BA = _permutation[B] + Z;
        int BB = _permutation[B + 1] + Z;

        // Интерполируем между 8 градиентами куба
        return Lerp(w,
            Lerp(v,
                Lerp(u, Grad(_permutation[AA], x, y, z),
                    Grad(_permutation[BA], x - 1, y, z)),
                Lerp(u, Grad(_permutation[AB], x, y - 1, z),
                    Grad(_permutation[BB], x - 1, y - 1, z))),
            Lerp(v,
                Lerp(u, Grad(_permutation[AA + 1], x, y, z - 1),
                    Grad(_permutation[BA + 1], x - 1, y, z - 1)),
                Lerp(u, Grad(_permutation[AB + 1], x, y - 1, z - 1),
                    Grad(_permutation[BB + 1], x - 1, y - 1, z - 1))));
    }

    private static double Fade(double t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static double Lerp(double t, double a, double b)
    {
        return a + t * (b - a);
    }

    private static double Grad(int hash, double x, double y, double z)
    {
        int h = hash & 15;
        double u = h < 8 ? x : y;
        double v = h < 4 ? y : h == 12 || h == 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}
