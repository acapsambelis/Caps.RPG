using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;

namespace SNS.Data.DataSerializer
{
    public interface IDataRelation
    {
        Type Type1 { get; }
        Type Type2 { get; }
        string ToXml(IDataObject SourceObject, bool UseBase64Arrays, IDataRelation[] Relations);
    }
    //public interface IJoin
    //{
    //    Type Type1 { get; }
    //    Type Type2 { get; }
    //    JoinType JoinMode { get; }
    //}

    public interface ILongIDDataObject
    {
        long ID { get; set; }
    }
    public interface IIDDataObject
    {
        int ID { get; set; }
    }
    public interface IDataObject
    {
    }
    public interface IGenericDataObject<T> : IDataObject where T : IGenericDataObject<T>, new()
    {
        bool WasLoaded { get; set; }

    }
    public interface IIDDataObject<T> : IIDDataObject, IGenericDataObject<T> where T : IIDDataObject<T>, new()
    {
    }
    public interface ILongIDDataObject<T> : ILongIDDataObject, IGenericDataObject<T> where T : ILongIDDataObject<T>, new()
    {
    }

}
