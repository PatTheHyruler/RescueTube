using System.Diagnostics.CodeAnalysis;

namespace RescueTube.Core.Utils.ExpressionUtils;

public record ScopedVariable<T>(T Value)
{
    public static implicit operator T(ScopedVariable<T> variable) => variable.Value;
}