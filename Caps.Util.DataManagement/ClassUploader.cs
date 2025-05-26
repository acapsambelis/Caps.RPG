//using System;
//using System.Data;
//using System.Data.SQLite;
//using System.Reflection;
//using System.Text;

//public class ClassUploader
//{
//    private readonly SQLiteConnection _connection;

//    public ClassUploader(string connectionString)
//    {
//        _connection = new SQLiteConnection(connectionString);
//        _connection.Open();
//    }

//    public void CreateTable<T>()
//    {
//        Type type = typeof(T);
//        var tableAttr = type.GetCustomAttribute<TableAttribute>();
//        if (tableAttr == null) throw new Exception("Missing TableAttribute on class.");

//        string tableName = tableAttr.Name;
//        var sb = new StringBuilder($"CREATE TABLE IF NOT EXISTS {tableName} (");

//        foreach (var prop in type.GetProperties())
//        {
//            var colAttr = prop.GetCustomAttribute<ColumnAttribute>();
//            if (colAttr == null) continue;

//            sb.Append($"{colAttr.Name} {MapType(prop.PropertyType)}");

//            if (colAttr.IsPrimaryKey) sb.Append(" PRIMARY KEY");
//            if (colAttr.AutoIncrement) sb.Append(" AUTOINCREMENT");

//            sb.Append(", ");
//        }

//        sb.Length -= 2; // remove last comma
//        sb.Append(");");

//        using var cmd = new SQLiteCommand(sb.ToString(), _connection);
//        cmd.ExecuteNonQuery();
//    }

//    public void Insert<T>(T obj)
//    {
//        Type type = typeof(T);
//        var tableAttr = type.GetCustomAttribute<TableAttribute>();
//        if (tableAttr == null) throw new Exception("Missing TableAttribute on class.");

//        string tableName = tableAttr.Name;

//        var cols = new StringBuilder();
//        var vals = new StringBuilder();
//        var parameters = new SQLiteCommand(_connection);

//        foreach (var prop in type.GetProperties())
//        {
//            var colAttr = prop.GetCustomAttribute<ColumnAttribute>();
//            if (colAttr == null || colAttr.AutoIncrement) continue;

//            string colName = colAttr.Name;
//            object value = prop.GetValue(obj);

//            cols.Append($"{colName}, ");
//            vals.Append($"@{colName}, ");
//            parameters.Parameters.AddWithValue($"@{colName}", value ?? DBNull.Value);
//        }

//        cols.Length -= 2;
//        vals.Length -= 2;

//        parameters.CommandText = $"INSERT INTO {tableName} ({cols}) VALUES ({vals})";
//        parameters.ExecuteNonQuery();
//    }

//    private string MapType(Type type)
//    {
//        if (type == typeof(int)) return "INTEGER";
//        if (type == typeof(long)) return "BIGINT";
//        if (type == typeof(string)) return "TEXT";
//        if (type == typeof(DateTime)) return "DATETIME";
//        if (type == typeof(bool)) return "BOOLEAN";
//        if (type == typeof(double)) return "REAL";
//        throw new NotSupportedException($"Type {type.Name} not supported.");
//    }
//}
