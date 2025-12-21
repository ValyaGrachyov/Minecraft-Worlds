namespace MinecraftWorldsAPI.Models;

public sealed class Chunk
{
    public const int SizeX = 16;
    public const int SizeZ = 16;
    public const int SectionSizeY = 16;

    public const int DefaultMinY = 0;
    public const int DefaultMaxY = 255;

    public ChunkPos Position { get; }

    public int MinY { get; }
    public int MaxY { get; }

    private readonly BlockInfo[,,] _blocks;
    private readonly Biome[,] _biomes;
    private readonly int[,] _heights;

    public Chunk(ChunkPos position, int minY = DefaultMinY, int maxY = DefaultMaxY)
    {
        Position = position;
        MinY = minY;
        MaxY = maxY;
        _blocks = new BlockInfo[SizeX, MaxY - MinY + 1, SizeZ];
        _biomes = new Biome[SizeX, SizeZ];
        _heights = new int[SizeX, SizeZ];
    }

    public void SetBlock(int x, int y, int z, BlockInfo block) => _blocks[x, y - MinY, z] = block;
    public BlockInfo GetBlock(int x, int y, int z) => _blocks[x, y - MinY, z];

    public void SetBiome(int x, int z, Biome biome) => _biomes[x, z] = biome;
    public Biome GetBiome(int x, int z) => _biomes[x, z];

    public void SetHeight(int x, int z, int height) => _heights[x, z] = height;
    public int GetHeight(int x, int z) => _heights[x, z];
}
