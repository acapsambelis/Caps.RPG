using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SNS.Data.DataSerializer.DataExtensions;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Globalization;
namespace SNS.Data.DataSerializer
{
    internal static class CommandUtility
    {
        internal class ExpressionParam
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public ExpressionParam(string ParamName, object ParamValue)
            {
                Name = ParamName;
                Value = ParamValue;
            }
        }
        private static string ParseOrderByExpression<T>(IDataConnector Connector, Expression exp, ref int ParamID, List<ExpressionParam> Parameters) where T : IGenericDataObject<T>, new()
        {
            string clause = "";

            if (exp.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methExp = (MethodCallExpression)exp;
                if (methExp.Method.Name == "DBAsc" && methExp.Object == null)
                {
                    if (methExp.Method.DeclaringType == typeof(DataObjectExtensions))
                    {
                        clause += ParseOrderByExpression<T>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + " ASC";
                    }
                }
                else if (methExp.Method.Name == "DBDesc" && methExp.Object == null)
                {
                    if (methExp.Method.DeclaringType == typeof(DataObjectExtensions))
                    {
                        clause += ParseOrderByExpression<T>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + " DESC";
                    }
                }
                else
                {
                    object obj = Expression.Lambda<Func<object>>(methExp).Compile()();
                    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, obj);
                    Parameters.Add(param);
                    ParamID++;
                    return param.Name;
                }
            }
            else if (exp.NodeType == ExpressionType.Convert)
            {
                return ParseOrderByExpression<T>(Connector, ((UnaryExpression)exp).Operand, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Modulo)
            {
                clause = ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " % " + ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.And)
            {
                clause = ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " & " + ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.ExclusiveOr)
            {
                clause = ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " ^ " + ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Or)
            {
                clause = ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " | " + ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Add)
            {
                BinaryExpression bExp = (BinaryExpression)exp;
                if (bExp.Left.Type == typeof(System.String) || bExp.Right.Type == typeof(System.String))
                {
                    clause = " CONCAT(" + ParseOrderByExpression<T>(Connector, bExp.Left, ref ParamID, Parameters) + "," + ParseOrderByExpression<T>(Connector, bExp.Right, ref ParamID, Parameters) + ") ";
                }
                else
                {
                    clause = ParseOrderByExpression<T>(Connector, bExp.Left, ref ParamID, Parameters);
                    clause += " + " + ParseOrderByExpression<T>(Connector, bExp.Right, ref ParamID, Parameters);
                }
            }
            else if (exp.NodeType == ExpressionType.Negate)
            {
                clause = ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " - " + ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Divide)
            {
                clause = ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " / " + ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Power)
            {
                clause = " POWER(" + ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " , " + ParseOrderByExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters) + ") ";
            }

            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression mexp = (MemberExpression)exp;
                DataProperty[] props = (DataProperty[])mexp.Member.GetCustomAttributes(typeof(DataProperty), true);

                if (props.Length > 0 && mexp.Expression is ParameterExpression)
                {

                    clause = ReflectionCache<T>.Schema != "" ? ReflectionCache<T>.Schema + "." : "";
                    clause += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                    return clause;

                    //IDataObject obj = Expression.Lambda<Func<IDataObject>>(mexp.Expression).Compile()();

                    //clause = obj.Schema() != "" ? obj.Schema() + "." : "";
                    //clause += Connector.Settings.TableSpecifierPrefix + obj.Table() + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                    //return clause;

                }
                else
                {
                    if (mexp.Expression is MemberExpression)
                    {
                        List<SubDataObject> subObjects = new List<SubDataObject>();

                        MemberExpression subExp = (MemberExpression)mexp.Expression;
                        while (subExp != null)
                        {
                            SubDataObject[] subObjs = (SubDataObject[])subExp.Member.GetCustomAttributes(typeof(SubDataObject), true);
                            if (subObjs.Length > 0)
                            {
                                subObjects.Insert(0, subObjs[0]);
                            }
                            subExp = subExp.Expression as MemberExpression;
                        }
                        if (subObjects.Count > 0 && props.Length > 0)
                        {
                            string fieldPrefix = "";
                            foreach (SubDataObject objInf in subObjects)
                                fieldPrefix += objInf.FieldPrefix;
                            clause = ReflectionCache<T>.Schema != "" ? ReflectionCache<T>.Schema + "." : "";
                            clause += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + fieldPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                            return clause;
                        }
                    }
                    //return ParseExpression<T>(Connector, mexp.Expression,ref ParamID, Parameters);
                    //throw new ApplicationException("Use of a non Data Property is not possible.");
                    //Not full proof method for all cases but will work for many
                    if (mexp.Expression.ToString().StartsWith("Convert("))
                    {
                        Type t = typeof(T);
                        PropertyInfo propInfo = t.GetProperty(mexp.Member.Name);
                        if (propInfo != null)
                        {
                            props = (DataProperty[])propInfo.GetCustomAttributes(typeof(DataProperty), true);
                            clause = ReflectionCache<T>.Schema != "" ? ReflectionCache<T>.Schema + "." : "";
                            clause += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                            return clause;
                        }
                    }
                    var obj = Expression.Lambda<Func<object>>(Expression.Convert(mexp, typeof(object))).Compile()();
                    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, obj);
                    Parameters.Add(param);
                    ParamID++;
                    return param.Name;

                    //if (mexp.Member is PropertyInfo)
                    //{
                    //    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, ((PropertyInfo)mexp.Member).GetValue(Object, null));
                    //    Parameters.Add(param);
                    //    ParamID++;
                    //    return param.Name;
                    //}
                }
            }
            else if (exp.NodeType == ExpressionType.Constant)
            {
                ConstantExpression cons = ((ConstantExpression)exp);
                //if (cons.Value is Enum)
                //{
                //    Type entype = cons.Value.GetType();
                //    Type undertype = Enum.GetUnderlyingType(entype);
                //    object o = Convert.ChangeType(cons.Value, undertype).ToString();
                //    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, o);
                //    Parameters.Add(param);
                //    return param.Name;
                //}
                //else
                //{

                ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, ((ConstantExpression)exp).Value);
                Parameters.Add(param);
                ParamID++;
                return param.Name;
                //}


            }
            else
            {
                throw new ApplicationException("Unable to convert lambda expression to SQL");
            }
            return clause;
        }
        private static string ParseSetExpression<T>(IDataConnector Connector, Expression exp, ref int ParamID, List<ExpressionParam> Parameters) where T : IGenericDataObject<T>, new()
        {
            string clause = "";

            if (exp.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methExp = (MethodCallExpression)exp;
                if (methExp.Method.Name == "DBSet")
                {
                    clause = ParseSetExpression<T>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + " = " + ParseSetExpression<T>(Connector, methExp.Arguments[1], ref ParamID, Parameters);
                }
                else
                {
                    object obj = Expression.Lambda<Func<object>>(methExp).Compile()();
                    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, obj);
                    Parameters.Add(param);
                    ParamID++;
                    return param.Name;

                    //String str = Expression.Lambda<Func<String>>(strExpr).Compile()();
                    //throw new ApplicationException("Method not supported for conversion to SQL:" + methExp.Method.Name + ".");
                }
            }
            else if (exp.NodeType == ExpressionType.Convert)
            {
                return ParseSetExpression<T>(Connector, ((UnaryExpression)exp).Operand, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Modulo)
            {
                clause = ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " % " + ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.And)
            {
                clause = ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " & " + ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.ExclusiveOr)
            {
                clause = ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " ^ " + ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Or)
            {
                clause = ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " | " + ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Add)
            {
                BinaryExpression bExp = (BinaryExpression)exp;
                if (bExp.Left.Type == typeof(System.String) || bExp.Right.Type == typeof(System.String))
                {
                    clause = " CONCAT(" + ParseSetExpression<T>(Connector, bExp.Left, ref ParamID, Parameters) + "," + ParseSetExpression<T>(Connector, bExp.Right, ref ParamID, Parameters) + ") ";
                }
                else
                {
                    clause = ParseSetExpression<T>(Connector, bExp.Left, ref ParamID, Parameters);
                    clause += " + " + ParseSetExpression<T>(Connector, bExp.Right, ref ParamID, Parameters);
                }
            }
            else if (exp.NodeType == ExpressionType.Negate)
            {
                clause = ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " - " + ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Divide)
            {
                clause = ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " / " + ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Power)
            {
                clause = " POWER(" + ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " , " + ParseSetExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters) + ") ";
            }

            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression mexp = (MemberExpression)exp;
                DataProperty[] props = (DataProperty[])mexp.Member.GetCustomAttributes(typeof(DataProperty), true);

                if (props.Length > 0 && mexp.Expression is ParameterExpression)
                {

                    clause = ReflectionCache<T>.Schema != "" ? ReflectionCache<T>.Schema + "." : "";
                    clause += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                    return clause;

                    //IDataObject obj = Expression.Lambda<Func<IDataObject>>(mexp.Expression).Compile()();

                    //clause = obj.Schema() != "" ? obj.Schema() + "." : "";
                    //clause += Connector.Settings.TableSpecifierPrefix + obj.Table() + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                    //return clause;

                }
                else
                {
                    if (mexp.Expression is MemberExpression)
                    {
                        List<SubDataObject> subObjects = new List<SubDataObject>();

                        MemberExpression subExp = (MemberExpression)mexp.Expression;
                        while (subExp != null)
                        {
                            SubDataObject[] subObjs = (SubDataObject[])subExp.Member.GetCustomAttributes(typeof(SubDataObject), true);
                            if (subObjs.Length > 0)
                            {
                                subObjects.Insert(0, subObjs[0]);
                            }
                            subExp = subExp.Expression as MemberExpression;
                        }
                        if (subObjects.Count > 0 && props.Length > 0)
                        {
                            string fieldPrefix = "";
                            foreach (SubDataObject objInf in subObjects)
                                fieldPrefix += objInf.FieldPrefix;
                            clause = ReflectionCache<T>.Schema != "" ? ReflectionCache<T>.Schema + "." : "";
                            clause += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + fieldPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                            return clause;
                        }
                    }
                    //return ParseExpression<T>(Connector, mexp.Expression,ref ParamID, Parameters);
                    //throw new ApplicationException("Use of a non Data Property is not possible.");
                    //Not full proof method for all cases but will work for many
                    if (mexp.Expression.ToString().StartsWith("Convert("))
                    {
                        Type t = typeof(T);
                        PropertyInfo propInfo = t.GetProperty(mexp.Member.Name);
                        if (propInfo != null)
                        {
                            props = (DataProperty[])propInfo.GetCustomAttributes(typeof(DataProperty), true);
                            clause = ReflectionCache<T>.Schema != "" ? ReflectionCache<T>.Schema + "." : "";
                            clause += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                            return clause;
                        }
                    }
                    var obj = Expression.Lambda<Func<object>>(Expression.Convert(mexp, typeof(object))).Compile()();
                    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, obj);
                    Parameters.Add(param);
                    ParamID++;
                    return param.Name;

                    //if (mexp.Member is PropertyInfo)
                    //{
                    //    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, ((PropertyInfo)mexp.Member).GetValue(Object, null));
                    //    Parameters.Add(param);
                    //    ParamID++;
                    //    return param.Name;
                    //}
                }
            }
            else if (exp.NodeType == ExpressionType.Constant)
            {
                ConstantExpression cons = ((ConstantExpression)exp);
                //if (cons.Value is Enum)
                //{
                //    Type entype = cons.Value.GetType();
                //    Type undertype = Enum.GetUnderlyingType(entype);
                //    object o = Convert.ChangeType(cons.Value, undertype).ToString();
                //    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, o);
                //    Parameters.Add(param);
                //    return param.Name;
                //}
                //else
                //{

                ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, ((ConstantExpression)exp).Value);
                Parameters.Add(param);
                ParamID++;
                return param.Name;
                //}


            }
            else
            {
                throw new ApplicationException("Unable to convert lambda expression to SQL");
            }
            return clause;
        }
        private static string ParseWhereExpression<T>(IDataConnector Connector, Expression exp, ref int ParamID, List<ExpressionParam> Parameters) where T : IGenericDataObject<T>, new()
        {
            string clause = "";
            if (exp.NodeType == ExpressionType.AndAlso)
            {
                clause = " (" + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " AND " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters) + ") ";
            }
            else if (exp.NodeType == ExpressionType.Lambda)
            {
                return ParseWhereExpression<T>(Connector, ((LambdaExpression)exp).Body, ref ParamID, Parameters);

            }
            else if (exp.NodeType == ExpressionType.OrElse)
            {
                clause = " (" + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " OR " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters) + ") ";
            }
            else if (exp.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methExp = (MethodCallExpression)exp;
                if (methExp.Method.Name == "Contains" && methExp.Object != null &&
                    methExp.Object.Type == typeof(string))
                {
                    clause = ParseWhereExpression<T>(Connector, methExp.Object, ref ParamID, Parameters) + " LIKE CONCAT('%'," + ParseWhereExpression<T>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + ",'%')";
                }
                else if (methExp.Method.Name == "Contains" &&
                    methExp.Object == null)
                {
                    if (methExp.Method.DeclaringType == typeof(Enumerable))
                    {
                        object obj = Expression.Lambda<Func<object>>(methExp.Arguments[0]).Compile()();
                        if (obj is IEnumerable)
                        {
                            IEnumerable set = (IEnumerable)obj;
                            string inSet = "";
                            foreach (object o in set)
                            {
                                inSet += Connector.Settings.VariableSymbol + "Param" + ParamID + ", ";
                                ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, o);
                                Parameters.Add(param);
                                ParamID++;
                            }
                            if (inSet.Length > 0)
                            {
                                inSet = inSet.Remove(inSet.Length - 2, 2);
                                clause = ParseWhereExpression<T>(Connector, methExp.Arguments[1], ref ParamID, Parameters) + " IN (" + inSet + ")";
                            }
                            else
                            {
                                clause = " 1 = 0";
                            }

                        }
                    }
                }
                else if (methExp.Method.Name == "DBIn" && methExp.Object == null)
                {
                    if (methExp.Method.DeclaringType == typeof(DataObjectExtensions))
                    {

                        object obj = Expression.Lambda<Func<object>>(methExp.Arguments[1]).Compile()();
                        bool useParameters = Expression.Lambda<Func<bool>>(methExp.Arguments[2]).Compile()();
                        if (obj is IEnumerable)
                        {
                            IEnumerable set = (IEnumerable)obj;
                            string inSet = "";
                            foreach (object o in set)
                            {
                                if (useParameters)
                                {
                                    inSet += Connector.Settings.VariableSymbol + "Param" + ParamID + ", ";
                                    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, o);
                                    Parameters.Add(param);
                                    ParamID++;
                                }
                                else
                                {
                                    if (o is string)
                                    {
                                        inSet += "'" + o + "', ";
                                    }
                                    else
                                    {
                                        inSet += o.ToString() + ", ";
                                    }
                                }
                            }
                            if (inSet.Length > 0)
                            {
                                inSet = inSet.Remove(inSet.Length - 2, 2);
                                clause = ParseWhereExpression<T>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + " IN (" + inSet + ")";
                            }
                            else
                            {
                                clause = " 1 = 0";
                            }

                        }
                    }
                }
                else if (methExp.Method.Name == "DBLike" && methExp.Object == null)
                {
                    if (methExp.Method.DeclaringType == typeof(DataObjectExtensions))
                    {
                        clause = ParseWhereExpression<T>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + " LIKE " + ParseWhereExpression<T>(Connector, methExp.Arguments[1], ref ParamID, Parameters);
                    }
                }
                else if (methExp.Method.Name == "StartsWith" &&
                   methExp.Object.Type == typeof(string))
                {
                    clause = ParseWhereExpression<T>(Connector, methExp.Object, ref ParamID, Parameters) + " LIKE CONCAT(" + ParseWhereExpression<T>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + ",'%')";
                }
                else if (methExp.Method.Name == "EndsWith" &&
                 methExp.Object.Type == typeof(string))
                {
                    clause = ParseWhereExpression<T>(Connector, methExp.Object, ref ParamID, Parameters) + " LIKE CONCAT('%'," + ParseWhereExpression<T>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + ")";
                }
                else
                {
                    object obj = Expression.Lambda<Func<object>>(methExp).Compile()();
                    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, obj);
                    Parameters.Add(param);
                    ParamID++;
                    return param.Name;

                    //String str = Expression.Lambda<Func<String>>(strExpr).Compile()();
                    //throw new ApplicationException("Method not supported for conversion to SQL:" + methExp.Method.Name + ".");
                }
            }
            else if (exp.NodeType == ExpressionType.Convert)
            {
                return ParseWhereExpression<T>(Connector, ((UnaryExpression)exp).Operand, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Equal)
            {
                string eqClause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                int paramCount = Parameters.Count;
                //clause = 
                eqClause += " = " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
                if (paramCount < Parameters.Count)
                {
                    if (Parameters.Last().Value == null)
                    {
                        eqClause = eqClause.Replace(" = ", " IS ");
                    }
                }
                clause += eqClause;
            }
            else if (exp.NodeType == ExpressionType.NotEqual)
            {
                string neqClause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                int paramCount = Parameters.Count;
                //clause = 
                neqClause += " != " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
                if (paramCount < Parameters.Count)
                {
                    if (Parameters.Last().Value == null)
                    {
                        neqClause = neqClause.Replace(" != ", " IS NOT ");
                    }
                }
                clause += neqClause;
            }
            else if (exp.NodeType == ExpressionType.GreaterThan)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " > " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.LessThan)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " < " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " >= " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.LessThanOrEqual)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " <= " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Modulo)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " % " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.And)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " & " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.ExclusiveOr)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " ^ " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Or)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " | " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Add)
            {
                BinaryExpression bExp = (BinaryExpression)exp;
                if (bExp.Left.Type == typeof(System.String) || bExp.Right.Type == typeof(System.String))
                {
                    clause = " CONCAT(" + ParseWhereExpression<T>(Connector, bExp.Left, ref ParamID, Parameters) + "," + ParseWhereExpression<T>(Connector, bExp.Right, ref ParamID, Parameters) + ") ";
                }
                else
                {
                    clause = ParseWhereExpression<T>(Connector, bExp.Left, ref ParamID, Parameters);
                    clause += " + " + ParseWhereExpression<T>(Connector, bExp.Right, ref ParamID, Parameters);
                }
            }
            else if (exp.NodeType == ExpressionType.Negate)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " - " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Divide)
            {
                clause = ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " / " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Power)
            {
                clause = " POWER(" + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " , " + ParseWhereExpression<T>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters) + ") ";
            }

            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression mexp = (MemberExpression)exp;
                DataProperty[] props = (DataProperty[])mexp.Member.GetCustomAttributes(typeof(DataProperty), true);

                if (props.Length > 0 && mexp.Expression is ParameterExpression)
                {

                    clause = ReflectionCache<T>.Schema != "" ? ReflectionCache<T>.Schema + "." : "";
                    clause += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                    return clause;

                    //IDataObject obj = Expression.Lambda<Func<IDataObject>>(mexp.Expression).Compile()();

                    //clause = obj.Schema() != "" ? obj.Schema() + "." : "";
                    //clause += Connector.Settings.TableSpecifierPrefix + obj.Table() + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                    //return clause;

                }
                else
                {
                    if (mexp.Expression is MemberExpression)
                    {
                        List<SubDataObject> subObjects = new List<SubDataObject>();

                        MemberExpression subExp = (MemberExpression)mexp.Expression;
                        while (subExp != null)
                        {
                            SubDataObject[] subObjs = (SubDataObject[])subExp.Member.GetCustomAttributes(typeof(SubDataObject), true);
                            if (subObjs.Length > 0)
                            {
                                subObjects.Insert(0, subObjs[0]);
                            }
                            subExp = subExp.Expression as MemberExpression;
                        }
                        if (subObjects.Count > 0 && props.Length > 0)
                        {
                            string fieldPrefix = "";
                            foreach (SubDataObject objInf in subObjects)
                                fieldPrefix += objInf.FieldPrefix;
                            clause = ReflectionCache<T>.Schema != "" ? ReflectionCache<T>.Schema + "." : "";
                            clause += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + fieldPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                            return clause;
                        }
                    }
                    //return ParseExpression<T>(Connector, mexp.Expression,ref ParamID, Parameters);
                    //throw new ApplicationException("Use of a non Data Property is not possible.");
                    //Not full proof method for all cases but will work for many
                    if (mexp.Expression.ToString().StartsWith("Convert("))
                    {
                        Type t = typeof(T);
                        PropertyInfo propInfo = t.GetProperty(mexp.Member.Name);
                        if (propInfo != null)
                        {
                            props = (DataProperty[])propInfo.GetCustomAttributes(typeof(DataProperty), true);
                            clause = ReflectionCache<T>.Schema != "" ? ReflectionCache<T>.Schema + "." : "";
                            clause += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                            return clause;
                        }
                    }
                    var obj = Expression.Lambda<Func<object>>(Expression.Convert(mexp, typeof(object))).Compile()();
                    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, obj);
                    Parameters.Add(param);
                    ParamID++;
                    return param.Name;

                    //if (mexp.Member is PropertyInfo)
                    //{
                    //    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, ((PropertyInfo)mexp.Member).GetValue(Object, null));
                    //    Parameters.Add(param);
                    //    ParamID++;
                    //    return param.Name;
                    //}
                }
            }
            else if (exp.NodeType == ExpressionType.Constant)
            {
                ConstantExpression cons = ((ConstantExpression)exp);
                //if (cons.Value is Enum)
                //{
                //    Type entype = cons.Value.GetType();
                //    Type undertype = Enum.GetUnderlyingType(entype);
                //    object o = Convert.ChangeType(cons.Value, undertype).ToString();
                //    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, o);
                //    Parameters.Add(param);
                //    return param.Name;
                //}
                //else
                //{

                ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, ((ConstantExpression)exp).Value);
                Parameters.Add(param);
                ParamID++;
                return param.Name;
                //}


            }
            else if (exp.NodeType == ExpressionType.New)
            {
                var obj = Expression.Lambda<Func<object>>(Expression.Convert(exp, typeof(object))).Compile()();
                ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, obj);
                Parameters.Add(param);
                ParamID++;
                return param.Name;
            }
            else
            {
                throw new ApplicationException("Unable to convert lambda expression to SQL");
            }
            return clause;
        }
        private static string ParseJoinClauseFromExpression<T1, T2>(IDataConnector Connector, Expression exp, ref int ParamID, List<ExpressionParam> Parameters)
            where T1 : IGenericDataObject<T1>, new()
            where T2 : IGenericDataObject<T2>, new()
        {
            string clause = "";
            if (exp.NodeType == ExpressionType.AndAlso)
            {
                clause = " (" + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " AND " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters) + ") ";
            }
            else if (exp.NodeType == ExpressionType.Lambda)
            {
                return ParseJoinClauseFromExpression<T1, T2>(Connector, ((LambdaExpression)exp).Body, ref ParamID, Parameters);

            }
            else if (exp.NodeType == ExpressionType.OrElse)
            {
                clause = " (" + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " OR " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters) + ") ";
            }
            else if (exp.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methExp = (MethodCallExpression)exp;
                if (methExp.Method.Name == "Contains" && methExp.Object != null &&
                    methExp.Object.Type == typeof(string))
                {
                    clause = ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Object, ref ParamID, Parameters) + " LIKE CONCAT('%'," + ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + ",'%')";
                }
                else if (methExp.Method.Name == "Contains" &&
                    methExp.Object == null)
                {
                    if (methExp.Method.DeclaringType == typeof(Enumerable))
                    {
                        object obj = Expression.Lambda<Func<object>>(methExp.Arguments[0]).Compile()();
                        if (obj is IEnumerable)
                        {
                            IEnumerable set = (IEnumerable)obj;
                            string inSet = "";
                            foreach (object o in set)
                            {
                                inSet += Connector.Settings.VariableSymbol + "Param" + ParamID + ", ";
                                ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, o);
                                Parameters.Add(param);
                                ParamID++;
                            }
                            if (inSet.Length > 0)
                            {
                                inSet = inSet.Remove(inSet.Length - 2, 2);
                                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Arguments[1], ref ParamID, Parameters) + " IN (" + inSet + ")";
                            }
                            else
                            {
                                clause = " 1 = 0";
                            }
                        }
                    }
                }
                else if (methExp.Method.Name == "DBIn" && methExp.Object == null)
                {
                    if (methExp.Method.DeclaringType == typeof(DataObjectExtensions))
                    {
                        object obj = Expression.Lambda<Func<object>>(methExp.Arguments[1]).Compile()();
                        if (obj is IEnumerable)
                        {
                            IEnumerable set = (IEnumerable)obj;
                            string inSet = "";
                            foreach (object o in set)
                            {
                                inSet += Connector.Settings.VariableSymbol + "Param" + ParamID + ", ";
                                ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, o);
                                Parameters.Add(param);
                                ParamID++;
                            }
                            if (inSet.Length > 0)
                            {
                                inSet = inSet.Remove(inSet.Length - 2, 2);
                                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + " IN (" + inSet + ")";
                            }
                            else
                            {
                                clause = " 1 = 0";
                            }

                        }
                    }
                }
                else if (methExp.Method.Name == "DBLike" && methExp.Object == null)
                {
                    if (methExp.Method.DeclaringType == typeof(DataObjectExtensions))
                    {
                        clause = ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + " LIKE " + ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Arguments[1], ref ParamID, Parameters);
                    }
                }
                else if (methExp.Method.Name == "StartsWith" &&
                   methExp.Object.Type == typeof(string))
                {
                    clause = ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Object, ref ParamID, Parameters) + " LIKE CONCAT(" + ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + ",'%')";
                }
                else if (methExp.Method.Name == "EndsWith" &&
                 methExp.Object.Type == typeof(string))
                {
                    clause = ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Object, ref ParamID, Parameters) + " LIKE CONCAT('%'," + ParseJoinClauseFromExpression<T1, T2>(Connector, methExp.Arguments[0], ref ParamID, Parameters) + ")";
                }
                else
                {
                    object obj = Expression.Lambda<Func<object>>(methExp).Compile()();
                    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, obj);
                    Parameters.Add(param);
                    ParamID++;
                    return param.Name;

                    //String str = Expression.Lambda<Func<String>>(strExpr).Compile()();
                    //throw new ApplicationException("Method not supported for conversion to SQL:" + methExp.Method.Name + ".");
                }
            }
            else if (exp.NodeType == ExpressionType.Convert)
            {
                return ParseJoinClauseFromExpression<T1, T2>(Connector, ((UnaryExpression)exp).Operand, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Equal)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " = " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.NotEqual)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " != " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.GreaterThan)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " > " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.LessThan)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " < " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " >= " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.LessThanOrEqual)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " <= " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Modulo)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " % " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.And)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " & " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.ExclusiveOr)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " ^ " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Or)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " | " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Add)
            {
                BinaryExpression bExp = (BinaryExpression)exp;
                if (bExp.Left.Type == typeof(System.String) || bExp.Right.Type == typeof(System.String))
                {
                    clause = " CONCAT(" + ParseJoinClauseFromExpression<T1, T2>(Connector, bExp.Left, ref ParamID, Parameters) + "," + ParseJoinClauseFromExpression<T1, T2>(Connector, bExp.Right, ref ParamID, Parameters) + ") ";
                }
                else
                {
                    clause = ParseJoinClauseFromExpression<T1, T2>(Connector, bExp.Left, ref ParamID, Parameters);
                    clause += " + " + ParseJoinClauseFromExpression<T1, T2>(Connector, bExp.Right, ref ParamID, Parameters);
                }
            }
            else if (exp.NodeType == ExpressionType.Negate)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " - " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Divide)
            {
                clause = ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " / " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters);
            }
            else if (exp.NodeType == ExpressionType.Power)
            {
                clause = " POWER(" + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Left, ref ParamID, Parameters);
                clause += " , " + ParseJoinClauseFromExpression<T1, T2>(Connector, ((BinaryExpression)exp).Right, ref ParamID, Parameters) + ") ";
            }

            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression mexp = (MemberExpression)exp;
                DataProperty[] props = (DataProperty[])mexp.Member.GetCustomAttributes(typeof(DataProperty), true);

                if (props.Length > 0 && mexp.Expression is ParameterExpression)
                {

                    clause = ReflectionUtility.GetSchema(mexp.Expression.Type) != "" ? ReflectionUtility.GetSchema(mexp.Expression.Type) + "." : "";
                    clause += Connector.Settings.TableSpecifierPrefix + ReflectionUtility.GetTable(mexp.Expression.Type) + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                    return clause;

                    //IDataObject obj = Expression.Lambda<Func<IDataObject>>(mexp.Expression).Compile()();

                    //clause = obj.Schema() != "" ? obj.Schema() + "." : "";
                    //clause += Connector.Settings.TableSpecifierPrefix + obj.Table() + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                    //return clause;

                }
                else
                {
                    if (mexp.Expression is MemberExpression)
                    {
                        List<SubDataObject> subObjects = new List<SubDataObject>();

                        MemberExpression subExp = (MemberExpression)mexp.Expression;
                        while (subExp != null)
                        {
                            SubDataObject[] subObjs = (SubDataObject[])subExp.Member.GetCustomAttributes(typeof(SubDataObject), true);
                            if (subObjs.Length > 0)
                            {
                                subObjects.Insert(0, subObjs[0]);
                            }
                            subExp = subExp.Expression as MemberExpression;
                        }
                        if (subObjects.Count > 0 && props.Length > 0)
                        {
                            string fieldPrefix = "";
                            foreach (SubDataObject objInf in subObjects)
                                fieldPrefix += objInf.FieldPrefix;
                            clause = ReflectionUtility.GetSchema(mexp.Expression.Type) != "" ? ReflectionUtility.GetSchema(mexp.Expression.Type) + "." : "";
                            clause += Connector.Settings.TableSpecifierPrefix + ReflectionUtility.GetTable(mexp.Expression.Type) + Connector.Settings.TableSpecifierSuffix + "." + Connector.Settings.FieldSpecifierPrefix + fieldPrefix + props[0].GetColumn + Connector.Settings.FieldSpecifierSuffix;
                            return clause;
                        }
                    }
                    //return ParseExpression<T>(Connector, mexp.Expression,ref ParamID, Parameters);
                    //throw new ApplicationException("Use of a non Data Property is not possible.");

                    var obj = Expression.Lambda<Func<object>>(Expression.Convert(mexp, typeof(object))).Compile()();
                    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, obj);
                    Parameters.Add(param);
                    ParamID++;
                    return param.Name;

                    //if (mexp.Member is PropertyInfo)
                    //{
                    //    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, ((PropertyInfo)mexp.Member).GetValue(Object, null));
                    //    Parameters.Add(param);
                    //    ParamID++;
                    //    return param.Name;
                    //}
                }
            }
            else if (exp.NodeType == ExpressionType.Constant)
            {
                ConstantExpression cons = ((ConstantExpression)exp);
                //if (cons.Value is Enum)
                //{
                //    Type entype = cons.Value.GetType();
                //    Type undertype = Enum.GetUnderlyingType(entype);
                //    object o = Convert.ChangeType(cons.Value, undertype).ToString();
                //    ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, o);
                //    Parameters.Add(param);
                //    return param.Name;
                //}
                //else
                //{

                ExpressionParam param = new ExpressionParam(Connector.Settings.VariableSymbol + "Param" + ParamID, ((ConstantExpression)exp).Value);
                Parameters.Add(param);
                ParamID++;
                return param.Name;
                //}


            }
            else
            {
                throw new ApplicationException("Unable to convert lambda expression to SQL");
            }
            return clause;
        }
        public static string BuildWhereClauseFromExpression<T>(IDataConnector Connector, Expression Method, out ExpressionParam[] Parameters, ref int ParamStart) where T : IGenericDataObject<T>, new()
        {
            string whereClause = "";
            //Type paramType = method.Parameters[0].Type;  // first parameter of expression
            //var d = paramType.GetMember((method.Body as MemberExpression).Member.Name)[0];
            List<ExpressionParam> parameters = new List<ExpressionParam>();
            whereClause = ParseWhereExpression<T>(Connector, Method, ref ParamStart, parameters);
            Parameters = parameters.ToArray();
            return whereClause;
        }
        public static string BuildJoinClauseFromExpression<T1, T2>(IDataConnector Connector, Expression<Func<T1, T2, bool>> Method, out ExpressionParam[] Parameters, ref int ParamStart)
            where T1 : IGenericDataObject<T1>, new()
            where T2 : IGenericDataObject<T2>, new()
        {
            string whereClause = "";
            //Type paramType = method.Parameters[0].Type;  // first parameter of expression
            //var d = paramType.GetMember((method.Body as MemberExpression).Member.Name)[0];
            List<ExpressionParam> parameters = new List<ExpressionParam>();
            whereClause = ParseJoinClauseFromExpression<T1, T2>(Connector, Method, ref ParamStart, parameters);
            Parameters = parameters.ToArray();
            return whereClause;
        }
        public static string BuildOrderByClauseFromExpression<T>(IDataConnector Connector, Expression<Func<T, object>> Method, out ExpressionParam[] Parameters) where T : IGenericDataObject<T>, new()
        {
            string orderByClause = "";
            int paramStart = 0;
            //Type paramType = method.Parameters[0].Type;  // first parameter of expression
            //var d = paramType.GetMember((method.Body as MemberExpression).Member.Name)[0];
            List<ExpressionParam> parameters = new List<ExpressionParam>();
            orderByClause = ParseOrderByExpression<T>(Connector, Method.Body, ref paramStart, parameters);
            Parameters = parameters.ToArray();
            return orderByClause;
        }
        public static string BuildOrderByClauseFromExpressions<T>(IDataConnector Connector, Expression<Func<T, object>>[] Methods, ref int ParamStart, out ExpressionParam[] Parameters) where T : IGenericDataObject<T>, new()
        {
            string orderByClause = "";
            List<ExpressionParam> parameters = new List<ExpressionParam>();
            foreach (Expression<Func<T, object>> e in Methods)
            {
                orderByClause += ParseOrderByExpression<T>(Connector, e.Body, ref ParamStart, parameters) + ", ";

            }
            if (orderByClause != "")
                orderByClause = orderByClause.Remove(orderByClause.Length - 2, 2);
            Parameters = parameters.ToArray();
            return orderByClause;
        }
        public static string BuildSetClauseFromExpressions<T>(IDataConnector Connector, Expression<Func<T, object>>[] Methods, ref int ParamStart, out ExpressionParam[] Parameters) where T : IGenericDataObject<T>, new()
        {
            string setClause = "";
            List<ExpressionParam> parameters = new List<ExpressionParam>();
            foreach (Expression<Func<T, object>> e in Methods)
            {
                setClause += ParseSetExpression<T>(Connector, e.Body, ref ParamStart, parameters) + ", ";

            }
            if (setClause != "")
                setClause = setClause.Remove(setClause.Length - 2, 2);
            Parameters = parameters.ToArray();
            return setClause;
        }
        public static string BuildWhereClauseFromKeys<T>(IGenericDataObject<T> Object, IDataConnector Connector) where T : IGenericDataObject<T>, new()
        {

            string whereClause = "";
            DataPropertyInfo[] keys = ReflectionCache<T>.GetKeys();
            if (keys.Length == 0)
            {
                throw new ApplicationException("Cannot build where clause with no keys. Either override the calling function or specify a data property with a key.");
            }
            foreach (DataPropertyInfo key in keys)
            {

                whereClause += (Connector.Settings.FieldSpecifierPrefix + key.FieldPrefixPath + key.PropertyAttribute.SetColumn + Connector.Settings.FieldSpecifierSuffix + " = " + Connector.Settings.VariableSymbol + key.PropertyAttribute.SetColumn + " AND ");
            }
            whereClause = whereClause.Remove(whereClause.Length - 5, 5);
            return whereClause;
        }
        //public static DataPropertyInfo[] GetKeys<T>(IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        //{
        //    DataPropertyInfo[] allProps = ReflectionCache<T>.GetDataProperties();
        //    List<DataPropertyInfo> keys = new List<DataPropertyInfo>();
        //    foreach (DataPropertyInfo propInf in allProps)
        //    {
        //        if (propInf.PropertyAttribute != null)
        //        {
        //            if (propInf.IsSubObject && !propInf.UseSubKeys)
        //                continue;

        //            if (propInf.PropertyAttribute.Key)
        //            {
        //                keys.Add(propInf);
        //            }
        //        }
        //    }
        //    return keys.ToArray();
        //}

        public static IDbCommand BuildInsertCommand<T>(IDbConnection con, IDataConnector Connector, IGenericDataObject<T> Object, bool SelectIdentity) where T : IGenericDataObject<T>, new()
        {
            DataPropertyInfo[] popMembers = ReflectionCache<T>.GetDataProperties();
            IDbCommand Command = con.CreateCommand();
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            if (popMembers.Length == 0)
            {
                return null;
            }
            StringBuilder sBuilder = new StringBuilder();
            StringBuilder vBuilder = new StringBuilder();
            sBuilder.Append("INSERT INTO ");
            vBuilder.Append(") VALUES(");
            if (ReflectionCache<T>.Schema != "")
                sBuilder.Append(ReflectionCache<T>.Schema + ".");
            sBuilder.Append(Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix);
            sBuilder.Append(" ( ");
            foreach (DataPropertyInfo popMember in popMembers)
            {
                if (popMember.PropertyAttribute != null)
                {
                    if (popMember.PropertyAttribute.Insert)
                    {
                        sBuilder.Append(Connector.Settings.TableSpecifierPrefix + popMember.FieldPrefixPath + popMember.PropertyAttribute.SetColumn + Connector.Settings.TableSpecifierSuffix + ", ");
                        vBuilder.Append(Connector.Settings.VariableSymbol + popMember.PropertyAttribute.SetColumn + ", ");

                        IDataParameter param = Command.CreateParameter();
                        param.ParameterName = Connector.Settings.VariableSymbol + popMember.PropertyAttribute.SetColumn;
                        param.Value = Object.GetDataPropertyValue(Connector, popMember);
                        Command.Parameters.Add(param);

                    }
                    else if (popMember.PropertyAttribute.Key)
                    {
                        IDataParameter param = Command.CreateParameter();
                        param.ParameterName = Connector.Settings.VariableSymbol + popMember.PropertyAttribute.SetColumn;
                        param.Value = Object.GetDataPropertyValue(Connector, popMember);

                        Command.Parameters.Add(param);
                    }
                }

            }
            sBuilder.Remove(sBuilder.Length - 2, 2);
            sBuilder.Append(vBuilder.ToString());
            sBuilder.Remove(sBuilder.Length - 2, 2);
            sBuilder.Append(")");
            Command.Connection = con;

            // PNG Changed to use SCOPE_IDENTITY() so we don't pick up the ID from a trigger insert
            Command.CommandText = sBuilder.ToString() + (SelectIdentity ? "; " + "SELECT " + Connector.Settings.LastIDCommand : "");
            return Command;

        }
        public static IDbCommand BuildUpdateCommand<T>(IDbConnection con, IDataConnector Connector, string[] UpdateProperties, object[] UpdateValues, string WhereClause) where T : IGenericDataObject<T>, new()
        {
            DataPropertyInfo[] popMembers = ReflectionCache<T>.GetDataProperties();
            if (popMembers.Length == 0)
            {
                return null;
            }
            IDbCommand Command = con.CreateCommand();
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            StringBuilder sBuilder = new StringBuilder();

            sBuilder.Append("UPDATE ");
            if (ReflectionCache<T>.Schema != "")
                sBuilder.Append(ReflectionCache<T>.Schema + ".");
            sBuilder.Append(Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix);
            sBuilder.Append(" SET ");

            foreach (DataPropertyInfo popMember in popMembers)
            {
                if (popMember.PropertyAttribute != null)
                {
                    if (popMember.PropertyAttribute.Update)
                    {
                        int updateAttrib = -1;
                        for (int i = 0; i < UpdateProperties.Length; i++)
                        {
                            if (popMember.PropertyInfo.Name == UpdateProperties[i])
                            {
                                updateAttrib = i;
                                break;
                            }
                        }
                        if (updateAttrib > -1)
                        {
                            sBuilder.Append(Connector.Settings.FieldSpecifierPrefix + popMember.FieldPrefixPath + popMember.PropertyAttribute.SetColumn + Connector.Settings.FieldSpecifierSuffix + "= " + Connector.Settings.VariableSymbol + popMember.PropertyAttribute.SetColumn + ", ");

                            IDataParameter param = Command.CreateParameter();
                            param.ParameterName = Connector.Settings.VariableSymbol + popMember.PropertyAttribute.SetColumn;
                            if (UpdateValues[updateAttrib] == null)
                                param.Value = DBNull.Value;
                            else
                                param.Value = UpdateValues[updateAttrib];

                            Command.Parameters.Add(param);
                        }
                    }
                }
            }
            sBuilder.Remove(sBuilder.Length - 2, 2);



            if (WhereClause != "")
            {
                sBuilder.Append(" WHERE " + WhereClause);
            }


            Command.Connection = con;
            Command.CommandText = sBuilder.ToString();

            return Command;

        }

        public static IDbCommand BuildUpdateCommand<T>(IDbConnection con, IDataConnector Connector, IGenericDataObject<T> Object, string WhereClause) where T : IGenericDataObject<T>, new()
        {

            DataPropertyInfo[] popMembers = ReflectionCache<T>.GetDataProperties();
            IDbCommand Command = con.CreateCommand();
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            if (popMembers.Length == 0)
            {
                return null;
            }
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.Append("UPDATE ");
            if (ReflectionCache<T>.Schema != "")
                sBuilder.Append(ReflectionCache<T>.Schema + ".");
            sBuilder.Append(Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix);
            sBuilder.Append(" SET ");
            foreach (DataPropertyInfo popMember in popMembers)
            {
                if (popMember.PropertyAttribute != null)
                {
                    if (popMember.PropertyAttribute.Update)
                    {
                        sBuilder.Append(Connector.Settings.FieldSpecifierPrefix + popMember.FieldPrefixPath + popMember.PropertyAttribute.SetColumn + Connector.Settings.FieldSpecifierSuffix + "= " + Connector.Settings.VariableSymbol + popMember.PropertyAttribute.SetColumn + ", ");

                        IDataParameter param = Command.CreateParameter();
                        param.ParameterName = Connector.Settings.VariableSymbol + popMember.PropertyAttribute.SetColumn;
                        param.Value = Object.GetDataPropertyValue(Connector, popMember);
                        Command.Parameters.Add(param);
                    }
                    else if (popMember.PropertyAttribute.Key)
                    {
                        IDataParameter param = Command.CreateParameter();
                        param.ParameterName = Connector.Settings.VariableSymbol + popMember.PropertyAttribute.SetColumn;
                        param.Value = Object.GetDataPropertyValue(Connector, popMember);
                        Command.Parameters.Add(param);
                    }
                }
            }
            sBuilder.Remove(sBuilder.Length - 2, 2);

            if (WhereClause != "")
            {
                sBuilder.Append(" WHERE " + WhereClause);
            }

            Command.Connection = con;
            Command.CommandText = sBuilder.ToString();

            return Command;

        }
        public static IDbCommand BuildDeleteCommand<T>(IDbConnection con, IDataConnector Connector, string WhereClause) where T : IGenericDataObject<T>, new()
        {
            return BuildDeleteCommand<T>(con, Connector, null, WhereClause);
        }
        public static IDbCommand BuildDeleteCommand<T>(IDbConnection con, IDataConnector Connector, IGenericDataObject<T> Object, string WhereClause) where T : IGenericDataObject<T>, new()
        {
            DataPropertyInfo[] popMembers = ReflectionCache<T>.GetDataProperties();
            IDbCommand Command = con.CreateCommand();
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            if (popMembers.Length == 0)
            {
                return null;
            }
            string popCommand = "DELETE FROM ";

            if (ReflectionCache<T>.Schema != "")
                popCommand += ReflectionCache<T>.Schema + ".";
            popCommand += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix;

            foreach (DataPropertyInfo popMember in popMembers)
            {
                DataProperty useAtt = popMember.PropertyAttribute;
                if (Object != null)
                {
                    if (useAtt.Key)
                    {
                        IDataParameter param = Command.CreateParameter();
                        param.ParameterName = Connector.Settings.VariableSymbol + useAtt.SetColumn;
                        param.Value = Object.GetDataPropertyValue(Connector, popMember);
                        Command.Parameters.Add(param);
                    }
                }

            }

            if (WhereClause != "")
            {
                popCommand += " WHERE " + WhereClause;
            }

            Command.Connection = con;
            Command.CommandText = popCommand;

            return Command;

        }
        public static IDbCommand BuildDeleteCommandFromExpression<T>(IDbConnection Connection, IDataConnector Connector, Expression<Func<T, bool>> Expression) where T : IGenericDataObject<T>, new()
        {
            ExpressionParam[] param;
            int ParamStart = 0;
            IDbCommand Command = BuildDeleteCommand<T>(Connection, Connector, BuildWhereClauseFromExpression<T>(Connector, Expression, out param, ref ParamStart));
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            foreach (ExpressionParam p in param)
            {
                IDataParameter dbParam = Command.CreateParameter();
                dbParam.ParameterName = p.Name;
                if (p.Value == null)
                {
                    dbParam.Value = DBNull.Value;
                }
                else
                    dbParam.Value = p.Value;
                Command.Parameters.Add(dbParam);
            }
            return Command;

        }
        public static IDbCommand BuildSelectCommand<T>(IDbConnection con, IDataConnector Connector, int Limit, params Expression<Func<T, object>>[] OrderByExpressions) where T : IGenericDataObject<T>, new()
        {
            ExpressionParam[] orderByParam;
            int ParamStart = 0;
            IDbCommand Command = BuildSelectCommand<T>(con, Connector, "", BuildOrderByClauseFromExpressions(Connector, OrderByExpressions, ref ParamStart, out orderByParam), Limit);
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            foreach (ExpressionParam p in orderByParam)
            {
                IDataParameter dbParam = Command.CreateParameter();
                dbParam.ParameterName = p.Name;
                if (p.Value == null)
                    dbParam.Value = DBNull.Value;
                else
                    dbParam.Value = p.Value;
                Command.Parameters.Add(dbParam);
            }
            return Command;
        }

        public static IDbCommand BuildSelectCommandFromExpression<T>(IDbConnection con, IDataConnector Connector, Expression Method, int Limit, params Expression<Func<T, object>>[] OrderByExpressions) where T : IGenericDataObject<T>, new()
        {
            ExpressionParam[] param;
            ExpressionParam[] orderByParam;
            int ParamStart = 0;
            IDbCommand Command = BuildSelectCommand<T>(con, Connector, BuildWhereClauseFromExpression<T>(Connector, Method, out param, ref ParamStart), BuildOrderByClauseFromExpressions(Connector, OrderByExpressions, ref ParamStart, out orderByParam), Limit);
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            foreach (ExpressionParam p in param)
            {
                IDataParameter dbParam = Command.CreateParameter();
                dbParam.ParameterName = p.Name;
                if (p.Value == null)
                    dbParam.Value = DBNull.Value;
                else
                    dbParam.Value = p.Value;
                Command.Parameters.Add(dbParam);
            }
            foreach (ExpressionParam p in orderByParam)
            {
                IDataParameter dbParam = Command.CreateParameter();
                dbParam.ParameterName = p.Name;
                if (p.Value == null)
                    dbParam.Value = DBNull.Value;
                else
                    dbParam.Value = p.Value;
                Command.Parameters.Add(dbParam);
            }
            return Command;
        }
        public static IDbCommand BuildSelectCommand<T>(IDbConnection con, IDataConnector Connector, string WhereClause, string OrderByClause, int Limit) where T : IGenericDataObject<T>, new()
        {
            return BuildSelectCommand<T>(con, Connector, null, WhereClause, OrderByClause, Limit);
        }
        public static IDbCommand BuildSelectCommand<T>(IDbConnection con, IDataConnector Connector, IGenericDataObject<T> Object, string WhereClause, string OrderByClause, int Limit) where T : IGenericDataObject<T>, new()
        {
            DataPropertyInfo[] allProps = ReflectionCache<T>.GetDataProperties();
            if (allProps.Length == 0)
            {
                return null;
            }
            IDbCommand Command = con.CreateCommand();
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            StringBuilder sBuilder = new StringBuilder();
            sBuilder.Append("SELECT ");
            if (Connector.Settings.SelectLimitMode == ConnectorSettings.LimitMode.TopSyntax && Limit > 0)
                sBuilder.Append(" TOP " + Limit + " ");

            foreach (DataPropertyInfo popMember in allProps)
            {
                if (popMember.PropertyAttribute != null)
                {
                    DataProperty useAtt = popMember.PropertyAttribute;
                    if (useAtt.Select && useAtt.MeetsDatabaseVersion(Connector.DatabaseVersion))
                    {
                        sBuilder.Append(Connector.Settings.FieldSpecifierPrefix + popMember.FieldPrefixPath + useAtt.GetColumn + Connector.Settings.FieldSpecifierSuffix + ", ");

                    }
                    if (Object != null)
                    {
                        if (useAtt.Key && useAtt.MeetsDatabaseVersion(Connector.DatabaseVersion))
                        {
                            IDataParameter param = Command.CreateParameter();
                            param.ParameterName = Connector.Settings.VariableSymbol + useAtt.SetColumn;
                            param.Value = Object.GetDataPropertyValue(Connector, popMember);
                            Command.Parameters.Add(param);
                        }
                    }
                }

            }

            sBuilder.Remove(sBuilder.Length - 2, 2);
            sBuilder.Append(" FROM ");
            if (ReflectionCache<T>.Schema != "")
                sBuilder.Append(ReflectionCache<T>.Schema + ".");
            sBuilder.Append(Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix);

            if (WhereClause != "")
            {
                sBuilder.Append(" WHERE ");
                sBuilder.Append(WhereClause);
            }
            if (OrderByClause != "")
            {
                sBuilder.Append(" ORDER BY ");
                sBuilder.Append(OrderByClause);
            }
            if (Connector.Settings.SelectLimitMode == ConnectorSettings.LimitMode.LimitSyntax && Limit > 0)
                sBuilder.Append(" Limit " + Limit + " ");
            Command.CommandText = sBuilder.ToString();
            return Command;
        }
        public static IDbCommand BuildSelectCountCommandFromExpression<T>(IDbConnection con, IDataConnector Connector, Expression<Func<T, bool>> Method) where T : IGenericDataObject<T>, new()
        {
            ExpressionParam[] param;
            int ParamStart = 0;
            IDbCommand Command = BuildSelectCountCommand<T>(con, Connector, BuildWhereClauseFromExpression<T>(Connector, Method, out param, ref ParamStart));
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            foreach (ExpressionParam p in param)
            {
                IDataParameter dbParam = Command.CreateParameter();
                dbParam.ParameterName = p.Name;
                if (p.Value == null)
                    dbParam.Value = DBNull.Value;
                else
                    dbParam.Value = p.Value;
                Command.Parameters.Add(dbParam);
            }

            return Command;
        }
        public static IDbCommand BuildSelectCountCommand<T>(IDbConnection con, IDataConnector Connector, string WhereClause) where T : IGenericDataObject<T>, new()
        {
            IDbCommand Command = con.CreateCommand();
            string popCommand = "SELECT COUNT(*)";
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            popCommand += " FROM ";
            if (ReflectionCache<T>.Schema != "")
                popCommand += ReflectionCache<T>.Schema + ".";
            popCommand += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix;

            if (WhereClause != "")
            {
                popCommand += " WHERE " + WhereClause;
            }

            Command.CommandText = popCommand;
            return Command;
        }
        public static IDbCommand GetProcedureCommand(IDbConnection Con, IDataConnector Connector, string commandName, string[] ParamNames, object[] ParamValues)
        {
            if (ParamNames.Length != ParamValues.Length)
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");

            IDbCommand command = Con.CreateCommand();
            if (Connector.DefaultCommandTimeout != null)
                command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            command.CommandText = commandName;
            command.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < ParamValues.Length; i++)
            {
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = Connector.Settings.VariableSymbol + ParamNames[i];
                if (ParamValues[i] == null)
                {
                    param.DbType = DbType.Binary; //Required by MSSQL For binary types but seems to work fine for non-binary types
                    param.Value = DBNull.Value;
                }
                //else if (ParamValues[i] is System.Byte[] && ((System.Byte[])ParamValues[i]).Length == 0)
                //{//Workaround not good MSSQL Errors when passing binary type to sproc with regular DBNULL without Binary set
                //    param.DbType = DbType.Binary;
                //    param.Value = DBNull.Value;
                //}
                else
                {
                    param.Value = ParamValues[i];
                }
                command.Parameters.Add(param);
            }
            return command;
        }
        public static IDbCommand BuildUpdateCommandFromExpression<T>(IDbConnection Connection, IDataConnector Connector, Expression<Func<T, bool>> Expression, Expression<Func<T, object>>[] SetExpressions) where T : IGenericDataObject<T>, new()
        {
            ExpressionParam[] param;
            ExpressionParam[] setParams;
            int ParamStart = 0;
            IDbCommand Command = BuildUpdateCommand<T>(Connection, Connector, BuildSetClauseFromExpressions(Connector, SetExpressions, ref ParamStart, out setParams), BuildWhereClauseFromExpression<T>(Connector, Expression, out param, ref ParamStart));
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;

            foreach (ExpressionParam p in setParams)
            {
                IDataParameter dbParam = Command.CreateParameter();
                dbParam.ParameterName = p.Name;
                if (p.Value == null)
                    dbParam.Value = DBNull.Value;
                else
                    dbParam.Value = p.Value;
                Command.Parameters.Add(dbParam);
            }
            foreach (ExpressionParam p in param)
            {
                IDataParameter dbParam = Command.CreateParameter();
                dbParam.ParameterName = p.Name;
                if (p.Value == null)
                    dbParam.Value = DBNull.Value;
                else
                    dbParam.Value = p.Value;
                Command.Parameters.Add(dbParam);
            }

            return Command;
        }
        public static IDbCommand BuildUpdateCommand<T>(IDbConnection con, IDataConnector Connector, string SetClause, string WhereClause) where T : IGenericDataObject<T>, new()
        {
            DataPropertyInfo[] popMembers = ReflectionCache<T>.GetDataProperties();
            IDbCommand Command = con.CreateCommand();
            if (Connector.DefaultCommandTimeout != null)
                Command.CommandTimeout = Connector.DefaultCommandTimeout.Value;
            if (popMembers.Length == 0)
            {
                return null;
            }
            string popCommand = "UPDATE ";
            if (ReflectionCache<T>.Schema != "")
                popCommand += ReflectionCache<T>.Schema + ".";
            popCommand += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix;
            popCommand += " SET ";
            popCommand += SetClause;
            if (WhereClause != "")
            {
                popCommand += " WHERE " + WhereClause;
            }


            Command.Connection = con;
            Command.CommandText = popCommand;

            return Command;

        }
        public static IDbCommand GetTextCommand(IDbConnection Connection, IDataConnector Connector, string SQL, string[] ParamNames, object[] ParamValues)
        {
            if (ParamNames.Length != ParamValues.Length)
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");

            IDbCommand command = Connection.CreateCommand();
            command.CommandText = SQL;
            command.CommandType = CommandType.Text;
            if (Connector.DefaultCommandTimeout != null)
                command.CommandTimeout = Connector.DefaultCommandTimeout.Value;
            for (int i = 0; i < ParamValues.Length; i++)
            {
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = Connector.Settings.VariableSymbol + ParamNames[i];
                if (ParamValues[i] == null)
                {
                    param.Value = DBNull.Value;
                }
                else
                {
                    param.Value = ParamValues[i];
                }
                command.Parameters.Add(param);
            }
            return command;
        }


    }
}
