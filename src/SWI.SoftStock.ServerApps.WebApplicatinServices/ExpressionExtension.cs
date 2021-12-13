using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public static class ExpressionExtension
    {
        /// <summary>
        ///     http://stackoverflow.com/questions/13954070/linq-expression-building-for-entity-framework-involving-complex-object
        /// </summary>
        /// <typeparam name="T">T type</typeparam>
        /// <param name="predicates"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> BuildOr<T>(
            IEnumerable<Expression<Func<T, bool>>> predicates)
        {
            System.Linq.Expressions.Expression body = null;
            ParameterExpression p = null;
            Expression<Func<T, bool>> first = null;

            foreach (var item in predicates)
            {
                if (first == null)
                {
                    first = item;
                }
                else
                {
                    if (body == null)
                    {
                        body = first.Body;
                        p = first.Parameters[0];
                    }

                    ParameterExpression toReplace = item.Parameters[0];
                    System.Linq.Expressions.Expression itemBody = ReplacementVisitor.Transform(item, toReplace, p);
                    body = System.Linq.Expressions.Expression.OrElse(body, itemBody);
                }
            }

            if (first == null)
            {
                throw new ArgumentException("Sequence contains no elements.", "predicates");
            }

            return (body == null) ? first : System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, p);
        }

        public static Expression<Func<T, bool>> Inverse<T>(this Expression<Func<T, bool>> expression)
        {
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(System.Linq.Expressions.Expression.Not(expression.Body),
                                        expression.Parameters);
        }
    }
}