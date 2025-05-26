using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace SNS.Data.DataSerializer
{
    public interface IDataConnector
    {
        int Retries { get; set; }
        int RetrySeconds { get; set; }
        int? DefaultCommandTimeout { get; set; }
        Type ADOConnectionType { get; }
        ConnectorSettings Settings { get; }
        string ConnectionString { get; set; }
        IDbConnection OpenConnection();
        string Name { get; set; }
        int? DatabaseVersion { get; set; }
        IDataConnector Clone();
    }
}
