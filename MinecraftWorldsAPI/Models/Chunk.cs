namespace MinecraftWorldsAPI.Models;

public sealed class Chunk
{
    public const int SizeX = 16;
    public const int SizeZ = 16;
    
    public ChunkPos Position { get; }

    public int MinY { get; }
    public int MaxY { get; }

    private readonly Block[,,] _blocks;

    public Chunk(ChunkPos position, int minY, int maxY)
    {
        Position = position;
        MinY = minY;
        MaxY = maxY;
        _blocks = new Block[SizeX, MaxY - MinY + 1, SizeZ];
    }

    public Block this[int x, int y, int z]
    {
        get => _blocks[x, y - MinY, z];
        set => _blocks[x, y - MinY, z] = value;
    }
}
