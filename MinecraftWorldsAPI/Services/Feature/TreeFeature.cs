using MinecraftWorldsAPI.Interfaces;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services.Feature;

public sealed class TreeFeature : IFeature
{
    private const int MinHeight = 4;
    private const int MaxHeight = 6;

    public void Apply(FeatureContext context)
    {
        var random = context.Random;
        var chunk = context.Chunk;

        // шанс появления дерева в чанке
        if (random.NextDouble() > 0.15)
            return;

        var x = random.NextInt(0, Chunk.SizeX);
        var z = random.NextInt(0, Chunk.SizeZ);

        var groundY = FindGround(chunk, x, z);
        if (groundY == null)
            return;

        var height = random.NextInt(MinHeight, MaxHeight + 1);

        GenerateTrunk(chunk, x, groundY.Value + 1, z, height);
        GenerateLeaves(chunk, x, groundY.Value + height, z);
    }

    private static int? FindGround(Chunk chunk, int x, int z)
    {
        for (int y = chunk.MaxY; y >= chunk.MinY; y--)
        {
            var block = chunk[x, y, z];
            if (block is Block.Grass or Block.Dirt)
                return y;
        }

        return null;
    }

    private static void GenerateTrunk(
        Chunk chunk,
        int x,
        int startY,
        int z,
        int height)
    {
        for (int y = startY; y < startY + height && y <= chunk.MaxY; y++)
        {
            chunk[x, y, z] = Block.Log;
        }
    }

    private static void GenerateLeaves(Chunk chunk, int x, int topY, int z)
    {
        for (int dx = -2; dx <= 2; dx++)
        for (int dz = -2; dz <= 2; dz++)
        for (int dy = -2; dy <= 0; dy++)
        {
            var lx = x + dx;
            var ly = topY + dy;
            var lz = z + dz;

            if (lx < 0 || lx >= Chunk.SizeX ||
                lz < 0 || lz >= Chunk.SizeZ ||
                ly < chunk.MinY || ly > chunk.MaxY)
                continue;

            if (Math.Abs(dx) + Math.Abs(dz) <= 3)
                chunk[lx, ly, lz] = Block.Leaves;
        }
    }
}
