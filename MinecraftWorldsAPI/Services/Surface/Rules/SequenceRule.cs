using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Surface;

namespace MinecraftWorldsAPI.Services.Surface.Rules;

/// <summary>
/// Правило последовательности
/// Проверяет правила по порядку, применяется только первое соответствующее правило
/// </summary>
public class SequenceRule : ISurfaceRule
{
    private readonly IReadOnlyList<ISurfaceRule> _rules;

    public SequenceRule(params ISurfaceRule[] rules)
    {
        _rules = rules;
    }

    public SequenceRule(IReadOnlyList<ISurfaceRule> rules)
    {
        _rules = rules;
    }

    public Block? Apply(SurfaceContext context)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Apply(context);
            if (result.HasValue)
            {
                return result;
            }
        }

        return null;
    }
}

