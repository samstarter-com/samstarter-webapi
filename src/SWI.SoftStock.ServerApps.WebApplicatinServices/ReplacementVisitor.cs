using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    /// <summary>
    /// http://stackoverflow.com/questions/13954070/linq-expression-building-for-entity-framework-involving-complex-object
    /// </summary>
    internal sealed class ReplacementVisitor : ExpressionVisitor
    {
        private IList<ParameterExpression> SourceParameters { get; set; }

        private System.Linq.Expressions.Expression ToFind { get; set; }

        private System.Linq.Expressions.Expression ReplaceWith { get; set; }

        public static System.Linq.Expressions.Expression Transform(
            LambdaExpression source,
            System.Linq.Expressions.Expression toFind,
            System.Linq.Expressions.Expression replaceWith)
        {
            var visitor = new ReplacementVisitor
            {
                SourceParameters = source.Parameters,
                ToFind = toFind,
                ReplaceWith = replaceWith,
            };

            return visitor.Visit(source.Body);
        }

        private System.Linq.Expressions.Expression ReplaceNode(System.Linq.Expressions.Expression node)
        {
            return (node == ToFind) ? ReplaceWith : node;
        }

        protected override System.Linq.Expressions.Expression VisitConstant(ConstantExpression node)
        {
            return ReplaceNode(node);
        }

        protected override System.Linq.Expressions.Expression VisitBinary(BinaryExpression node)
        {
            System.Linq.Expressions.Expression result = ReplaceNode(node);
            if (result == node)
            {
                result = base.VisitBinary(node);
            }
            return result;
        }

        protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression node)
        {
            if (SourceParameters.Contains(node))
            {
                return ReplaceNode(node);
            }
            return SourceParameters.FirstOrDefault(p => p.Name == node.Name) ?? node;
        }
    }
}