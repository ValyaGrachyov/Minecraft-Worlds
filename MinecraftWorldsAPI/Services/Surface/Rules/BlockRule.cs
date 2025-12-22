using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Surface;

namespace MinecraftWorldsAPI.Services.Surface.Rules;

/// <summary>
/// Правило размещения блока
/// Просто размещает указанный блок
/// </summary>
public class BlockRule : ISurfaceRule
{
    private readonly Block _block;

    public BlockRule(Block block)
    {
        _block = block;
    }

    public Block? Apply(SurfaceContext context)
    {
        return _block;
    }
}

