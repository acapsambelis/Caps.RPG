using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace SNS.Data.DataSerializer
{
    public class Join<T1, T2> : IJoin
        where T1 : IGenericDataObject<T1>, new()
        where T2 : IGenericDataObject<T2>, new()
    {
        private JoinType jType = JoinType.INNER;
        public Expression<Func<T1, T2, bool>> JoinExpression { get; set; }

        public Type Type1
        {
            get { return typeof(T1); }
        }

        public Type Type2
        {
            get { return typeof(T2); }
        }

        public JoinType JoinMode
        {
            get { return jType; }
            set { jType = value; }
        }
        public Join(Expression<Func<T1, T2, bool>> JoinExpression)
        {
            this.JoinExpression = JoinExpression;
        }
        public Join(JoinType JoinType, Expression<Func<T1, T2, bool>> JoinExpression)
        {
            jType = JoinType;
            this.JoinExpression = JoinExpression;
        }
    }
}
