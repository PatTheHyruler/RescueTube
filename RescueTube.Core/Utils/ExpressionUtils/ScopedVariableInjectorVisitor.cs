using System.Linq.Expressions;

namespace RescueTube.Core.Utils.ExpressionUtils;

public class ScopedVariableInjectorVisitor : ExpressionVisitor
{
    protected override Expression VisitUnary(UnaryExpression node)
    {
        var scopedVariableType = typeof(ScopedVariable<>);
        var declaringType = node.Method?.DeclaringType;
        if (IsSameGenericType(scopedVariableType, declaringType) && node.Method?.Name == "op_Implicit")
        {
            return Expression.Constant(GetValue(node));
        }

        return base.VisitUnary(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        var scopedVariableType = typeof(ScopedVariable<>);
        var declaringType = node.Member.DeclaringType;
        if (IsSameGenericType(scopedVariableType, declaringType))
        {
            return Expression.Constant(GetValue(node));
        }

        return base.VisitMember(node);
    }

    private static object GetValue(Expression expression)
    {
        return Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile()();
    }

    private static bool IsSameGenericType(Type? t1, Type? t2)
    {
        return t1?.Name == t2?.Name && t1?.Namespace == t2?.Namespace && t1?.Module == t2?.Module;
    }
}