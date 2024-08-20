using System.Linq.Expressions;

namespace RescueTube.Core.Utils.ExpressionUtils;

public static class ExpressionUtils
{
    private static readonly ScopedVariableInjectorVisitor Visitor = new();

    public static TExpression VisitScopedVariables<TExpression>(this TExpression expression)
        where TExpression : Expression
    {
        var result = Visitor.Visit(expression);
        if (result is not TExpression typedResult)
        {
            throw new ApplicationException(
                $"Visitor {nameof(ScopedVariableInjectorVisitor)} produced {result.Type}, but expected {typeof(TExpression)}");
        }

        return typedResult;
    }

    public static Expression<Func<TEntity, TResult>> SelectValue<TEntity, TKey, TResult>(
        Expression<Func<TEntity, TKey>> keySelector, Dictionary<TKey, TResult> valueDict)
        where TKey : notnull
    {
        var parameter = keySelector.Parameters[0];
        var keyColumnProperty = keySelector.Body;

        Expression caseExpression = Expression.Constant(null, typeof(TResult));

        caseExpression = valueDict.Aggregate(
            caseExpression,
            (current, kvp) => Expression.Condition(
                Expression.Equal(keyColumnProperty, Expression.Constant(kvp.Key)),
                Expression.Constant(kvp.Value),
                current
            )
        );

        return Expression.Lambda<Func<TEntity, TResult>>(caseExpression, parameter);
    }

    public static (Func<TSource, TProperty> Getter, Action<TSource, TProperty> Setter, string Name)
        GetGetterAndSetter<TSource, TProperty>(this Expression<Func<TSource, TProperty>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Invalid expression type", nameof(expression));
        }

        var getter = expression.Compile();
        
        var thisParameter = Expression.Parameter(typeof(TSource), "$this");
        var valueParameter = Expression.Parameter(typeof(TProperty), "value");
        var assign = Expression.Assign(Expression.MakeMemberAccess(thisParameter, memberExpression.Member),
            valueParameter);
        var setter = Expression
            .Lambda<Action<TSource, TProperty>>(assign, thisParameter, valueParameter)
            .Compile();
        return (getter, setter, memberExpression.Member.Name);
    }
}