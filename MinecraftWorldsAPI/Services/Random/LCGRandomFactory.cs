using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Random;

/// <summary>
/// Реализация IRandomFactory на основе алгоритма LCG (Linear Congruential Generator)
/// 
/// LCG - это простой и быстрый алгоритм генерации псевдослучайных чисел.
/// Формула: X(n+1) = (a * X(n) + c) mod m
/// 
/// Используемые параметры (из Numerical Recipes):
/// - a = 1664525 (множитель)
/// - c = 1013904223 (инкремент)
/// - m = 2^32 (модуль, достигается через маску 0xFFFFFFFF)
/// 
/// Преимущества:
/// - Очень быстрый алгоритм
/// - Детерминированный (одинаковый seed дает одинаковую последовательность)
/// - Небольшой размер состояния (одно число)
/// 
/// Недостатки:
/// - Относительно низкое качество случайности по сравнению с современными алгоритмами
/// - Известные паттерны в последовательности
/// - Не подходит для криптографических целей
/// 
/// Для генерации мира Minecraft этого достаточно, так как требуется только детерминированность
/// и приемлемое качество случайности для процедурной генерации.
/// </summary>
public class LCGRandomFactory : IRandomFactory
{
    public IRandom CreateRandom(long seed)
    {
        return new LCGRandom(seed);
    }

    public IRandom CreateForChunk(long worldSeed, ChunkPos chunkPos, long salt)
    {
        // Комбинируем seed для чанка используя хеш-функцию на основе простых чисел
        // Формула: seed = ((seed * 31 + X) * 31 + Z) * 31 + salt
        // Использование 31 - стандартная практика в Java (используется в String.hashCode())
        long combinedSeed = worldSeed;
        combinedSeed = combinedSeed * 31 + chunkPos.X;
        combinedSeed = combinedSeed * 31 + chunkPos.Z;
        combinedSeed = combinedSeed * 31 + salt;
        return new LCGRandom(combinedSeed);
    }
}

/// <summary>
/// Реализация IRandom на основе алгоритма LCG (Linear Congruential Generator)
/// 
/// Алгоритм LCG генерирует последовательность псевдослучайных чисел по формуле:
/// X(n+1) = (a * X(n) + c) mod m
/// 
/// Где:
/// - X(n) - текущее состояние (начальное значение = seed)
/// - a = 1664525 - множитель (выбран из Numerical Recipes для хороших статистических свойств)
/// - c = 1013904223 - инкремент (выбран из Numerical Recipes)
/// - m = 2^32 - модуль (достигается через побитовую маску 0xFFFFFFFF)
/// 
/// Период генератора: до 2^32 значений перед повторением последовательности
/// 
/// Качество: Подходит для процедурной генерации контента в играх, но не для криптографии
/// </summary>
internal class LCGRandom : IRandom
{
    private long _state;

    public LCGRandom(long seed)
    {
        Seed = seed;
        _state = seed;
    }

    public long Seed { get; }

    public uint NextUInt()
    {
        // Линейный конгруэнтный генератор (LCG)
        // Формула: X(n+1) = (a * X(n) + c) mod m
        // Параметры из Numerical Recipes (глава 7.1):
        // a = 1664525, c = 1013904223, m = 2^32
        _state = (_state * 1664525L + 1013904223L) & 0xFFFFFFFFL;
        return (uint)_state;
    }

    public int NextInt(int min, int max)
    {
        if (min >= max)
            return min;

        uint range = (uint)(max - min);
        if (range == 0)
            return min;

        uint randomValue = NextUInt();
        return min + (int)(randomValue % range);
    }

    public double NextDouble()
    {
        // Нормализуем случайное число к диапазону [0.0, 1.0)
        uint randomValue = NextUInt();
        return randomValue / (double)uint.MaxValue;
    }

    public IRandom Fork(long salt)
    {
        // Создаем новый генератор с модифицированным seed
        // Используется для создания независимых последовательностей случайных чисел
        long newSeed = Seed * 31 + salt;
        return new LCGRandom(newSeed);
    }
}

