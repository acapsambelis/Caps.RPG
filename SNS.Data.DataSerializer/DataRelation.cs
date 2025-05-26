using SNS.Data.DataSerializer.DataExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SNS.Data.DataSerializer.XmlExtensions;
namespace SNS.Data.DataSerializer
{
    public class DataRelation<T1, T2> : IDataRelation
        where T1 : IGenericDataObject<T1>, new()
        where T2 : IGenericDataObject<T2>, new()
    {

        public Expression<Func<T2, bool>> WhereExpression { get; set; }

        public string ConnectorName { get; set; }
        public Type Type1
        {
            get { return typeof(T1); }
        }

        public Type Type2
        {
            get { return typeof(T2); }
        }
        public Expression<Func<T2, object>>[] OrderByExpressions
        {
            get;set;
        }
        public T2[] GetRelated()
        {
            return Data<T2>.LoadMany(ConnectorName, WhereExpression, OrderByExpressions);
        }
        public string ToXml(IDataObject SourceObject, bool UseBase64Arrays, IDataRelation[] Relations)
        {
            T2[] results = GetRelated();
            return Xml<T2>.ToXml(results, UseBase64Arrays, Relations);

        }
        public DataRelation(Expression<Func<T2, bool>> WhereExpression, params Expression<Func<T2, object>>[] OrderByExpressions)
        {
            ConnectorName = ReflectionCache<T2>.ConnectorName;
            this.WhereExpression = WhereExpression;
            this.OrderByExpressions = OrderByExpressions;
        }
        public DataRelation(string ConnectorName, Expression<Func<T2, bool>> WhereExpression, params Expression<Func<T2, object>>[] OrderByExpressions)
        {
            this.ConnectorName = ConnectorName;
            this.WhereExpression = WhereExpression;
            this.OrderByExpressions = OrderByExpressions;
        }
    }
}
