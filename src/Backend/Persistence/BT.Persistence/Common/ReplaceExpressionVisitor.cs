using System.Linq.Expressions;

namespace BT.Persistence.Common;

internal sealed class ReplaceExpressionVisitor(Expression source, Expression target) : ExpressionVisitor
{
    public override Expression? Visit(Expression? node)
    {
        return node == source
            ? target
            : base.Visit(node);
    }
}
