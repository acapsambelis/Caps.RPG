using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace SNS.Data.DataSerializer
{
    public abstract class BaseOrderBy
    {
        internal abstract string BuildOrderByString(IDataConnector Connector, out CommandUtility.ExpressionParam[] Parameters);

    }
    public class OrderByExpression<T> : BaseOrderBy where T : IGenericDataObject<T>, new()
    {
        internal Expression<Func<T, object>> Expression { get; set; }
        internal override string BuildOrderByString(IDataConnector Connector, out CommandUtility.ExpressionParam[] Parameters)
        {
            return CommandUtility.BuildOrderByClauseFromExpression(Connector, Expression, out Parameters);
        }
        public static implicit operator OrderByExpression<T>(Expression<Func<T, object>> Expression)
        {
            return new OrderByExpression<T>(Expression);
        }
        public OrderByExpression(Expression<Func<T, object>> Expression)
        {
            this.Expression = Expression;
        }
    }
}
