using MinecraftWorldsAPI.Models;
using MinecraftWorldsAPI.Services.Surface;

namespace MinecraftWorldsAPI.Services.Surface.Rules;

/// <summary>
/// Правило с условием
/// Применяет правило только если условие выполнено
/// </summary>
public class ConditionRule : ISurfaceRule
{
    private readonly ISurfaceCondition _condition;
    private readonly ISurfaceRule _thenRule;

    public ConditionRule(ISurfaceCondition condition, ISurfaceRule thenRule)
    {
        _condition = condition;
        _thenRule = thenRule;
    }

    public Block? Apply(SurfaceContext context)
    {
        if (_condition.Test(context))
        {
            return _thenRule.Apply(context);
        }

        return null;
    }
}

