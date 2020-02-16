using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

namespace VSS.Hosted.VLCommon
{
  public class StoredProcDefinition
  {
    public static readonly ushort DB_NAME_LENGTH = 64;
    private static readonly int DEFAULT_COMMAND_TIMEOUT_SECS = 120;

    public string DbName{ get; set; }

    public StoredProcDefinition(string dbName, string storedProcName) : this(dbName,storedProcName,DEFAULT_COMMAND_TIMEOUT_SECS)
    {
    }

    public StoredProcDefinition(string dbName, string storedProcName, int timeoutSeconds)
    {
      DbName = dbName;
      Cmd.CommandText = storedProcName;
      Cmd.CommandType = CommandType.StoredProcedure;
      Cmd.CommandTimeout = timeoutSeconds;
    }

    public StoredProcDefinition(string dbName, bool isFunction, string functionName) : this(dbName,isFunction,functionName,DEFAULT_COMMAND_TIMEOUT_SECS)
    {
    }

    public StoredProcDefinition(string dbName, bool isFunction, string functionName, int timeoutSeconds)
    {
      DbName = dbName;
      Cmd.CommandText = functionName;
      Cmd.CommandType = CommandType.Text;
      Cmd.CommandTimeout = timeoutSeconds;
    }

    public void AddInput(string paramName, long? input)
    {
      Cmd.Parameters.Add(new SqlParameter(paramName, input ?? (object)DBNull.Value));
    }

    public void AddInput(string paramName, IEnumerable list)
    {
      AddInput(paramName, list, true);
    }

    public void AddInput(string paramName, DataTable dt)
    {
      SqlParameter tvpParam = Cmd.Parameters.AddWithValue(paramName, dt);
      tvpParam.SqlDbType = SqlDbType.Structured;
    }

    public void AddInput(string paramName, IEnumerable list, bool isUnicode)
    {
      if (null == list)
      {
        AddInput(paramName, (object)DBNull.Value);
      }
      else
      {
        StringBuilder csvList = new StringBuilder();
        Type enumType = typeof(Enum);
        foreach (object obj in list)
        {
          if (csvList.Length > 0)
            csvList.Append(",");
          if (obj.GetType().IsSubclassOf(enumType))
            csvList.Append((int)obj);
          else
            csvList.Append(obj.ToString());
        }

        SqlParameter param = new SqlParameter(paramName, isUnicode ? SqlDbType.NVarChar : SqlDbType.VarChar);
        param.Value = csvList.ToString();
        Cmd.Parameters.Add(param);
      }
    }

    public void AddInput(string paramName, long paramValue)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.BigInt);
      param.Value = paramValue;
      Cmd.Parameters.Add(param);
    }

    public void AddInput(string paramName, int paramValue)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Int);
      param.Value = paramValue;
      Cmd.Parameters.Add(param);
    }

    public void AddInput(string paramName, char paramValue)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Char);
      param.Value = paramValue;
      Cmd.Parameters.Add(param);
    }

    public void AddInput(string paramName, short paramValue)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.SmallInt);
      param.Value = paramValue;
      Cmd.Parameters.Add(param);
    }

    public void AddInput(string paramName, byte paramValue)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.TinyInt);
      param.Value = paramValue;
      Cmd.Parameters.Add(param);
    }

    public void AddInput(string paramName, bool paramValue)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Bit);
      param.Value = paramValue;
      Cmd.Parameters.Add(param);
    }

    public void AddInput(long paramValue)
    {
      AddInput(DefaultParamName(), paramValue);
    }

    internal void AddInput(string paramName, string paramValue, int maxLength)
    {
      int length = Math.Min(paramValue.Length, maxLength);
      AddInput(paramName, paramValue.Substring(0, length));
    }

    public void AddInput(string paramName, string paramValue)
    {
      if (null == paramValue)
      {
        SqlParameter param = new SqlParameter(paramName, (object)DBNull.Value);
        Cmd.Parameters.Add(param);
      }
      else
      {
        SqlParameter param = new SqlParameter(paramName, SqlDbType.NVarChar);
        param.Value = paramValue;
        Cmd.Parameters.Add(param);
      }
    }

    public void AddInput(string paramValue)
    {
      AddInput(DefaultParamName(), paramValue);
    }

    public void AddInput(string paramName, DateTime paramValue, bool isAccurate = false)
    {
      if (isAccurate)
      {
        SqlParameter param = new SqlParameter(paramName, SqlDbType.DateTime2);
        param.Value = paramValue;
        Cmd.Parameters.Add(param);
      }
      else
      {
        SqlParameter param = new SqlParameter(paramName, SqlDbType.DateTime);
        param.Value = new SqlDateTime(paramValue); //watch this conversion - sqldatetime structs are less precise than datetime structs.
        Cmd.Parameters.Add(param); 
      }
    }

    public void AddInput(string paramName, DateTime? input)
    {
      Cmd.Parameters.Add(new SqlParameter(paramName, input ?? (object)DBNull.Value));
    }

    public void AddInput(DateTime paramValue)
    {
      AddInput(DefaultParamName(), paramValue);
    }

    public void AddInput(string paramName, double paramValue)
    {
      if (paramValue <= Double.MinValue || paramValue >= Double.MaxValue ||
           double.IsNaN(paramValue) || double.IsInfinity(paramValue))
      {
        SqlParameter param = new SqlParameter(paramName, (object)DBNull.Value);
        Cmd.Parameters.Add(param);
      }
      else
      {
        SqlParameter param = new SqlParameter(paramName, SqlDbType.Float);
        param.Value = paramValue;
        Cmd.Parameters.Add(param);
      }
    }

    public void AddInput(double paramValue)
    {
      AddInput(DefaultParamName(), paramValue);
    }

    public void AddInput(string paramName, float paramValue)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Real);
      param.Value = paramValue;
      Cmd.Parameters.Add(param);
    }

    public void AddInput(float paramValue)
    {
      AddInput(DefaultParamName(), paramValue);
    }

    public void AddInput(string paramName, object paramValue)
    {
      SqlParameter param = null;
      if (paramValue == null)
        param = new SqlParameter(paramName, (object)DBNull.Value);
      else if (paramValue.GetType().IsSubclassOf(typeof(Enum)))
      {
        param = new SqlParameter(paramName, SqlDbType.Int);
        param.Value = (int)paramValue;
      }
      else
        param = new SqlParameter(paramName, paramValue);
      Cmd.Parameters.Add(param);
    }

    public void AddInput(object paramValue)
    {
      AddInput(DefaultParamName(), paramValue);
    }

    internal void AddInputTable(string paramName, List<SqlDataRecord> data, string typeName)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Structured);
      param.TypeName = typeName;
      param.Value = data;
      Cmd.Parameters.Add(param);
    }

    public void AddInputVarBinary(string paramName, byte[] bytes)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.VarBinary);
      param.Value = bytes;
      Cmd.Parameters.Add(param);
    }

    public void AddInputXml(string paramName, object paramValue)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Xml);
      param.Value = paramValue;
      Cmd.Parameters.Add(param);
    }

    public void AddInputImage(string paramName, byte[] bytes)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Image);
      param.Value = bytes;
      Cmd.Parameters.Add(param);
    }

    public void AddDateTimeOutput(string paramName)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.DateTime);
      param.Direction = ParameterDirection.Output;
      Cmd.Parameters.Add(param);
    }

    public void AddLongOutput(string paramName)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.BigInt);
      param.Direction = ParameterDirection.Output;
      Cmd.Parameters.Add(param);
    }

    public void AddLongOutput()
    {
      AddLongOutput(DefaultParamName());
    }

    public void AddIntOutput(string paramName)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Int);
      param.Direction = ParameterDirection.Output;
      Cmd.Parameters.Add(param);
    }

    public void AddBitOutput(string paramName)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Bit);
      param.Direction = ParameterDirection.Output;
      Cmd.Parameters.Add(param);
    }

    public void AddIntOutput()
    {
      AddIntOutput(DefaultParamName());
    }

    public void AddDoubleOutput(string paramName)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Float);
      param.Direction = ParameterDirection.Output;
      Cmd.Parameters.Add(param);
    }

    public void AddDoubleOutput()
    {
      AddLongOutput(DefaultParamName());
    }

    public void AddStringOutput(string paramName, ushort maxChars)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.NVarChar);
      param.Direction = ParameterDirection.Output;
      param.Size = maxChars;
      Cmd.Parameters.Add(param);
    }

    public void AddStringOutput(ushort maxChars)
    {
      AddStringOutput(DefaultParamName(), maxChars);
    }

    public void AddLongReturnValue(string paramName)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.BigInt);
      param.Direction = ParameterDirection.ReturnValue;
      Cmd.Parameters.Insert(0, param);
    }

    public void AddLongReturnValue()
    {
      AddLongReturnValue(DefaultParamName());
    }

    public void AddIntReturnValue(string paramName)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.Int);
      param.Direction = ParameterDirection.ReturnValue;
      Cmd.Parameters.Insert(0, param);
    }

    public void AddIntReturnValue()
    {
      AddIntReturnValue(DefaultParamName());
    }

    public void AddDateTimeReturnValue(string paramName)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.DateTime);
      param.Direction = ParameterDirection.ReturnValue;
      Cmd.Parameters.Insert(0, param);
    }

    public void AddDateTimeReturnValue()
    {
      AddDateTimeReturnValue(DefaultParamName());
    }

    public void AddStringReturn(string paramName, ushort maxChars)
    {
      SqlParameter param = new SqlParameter(paramName, SqlDbType.NVarChar);
      param.Direction = ParameterDirection.ReturnValue;
      param.Size = maxChars;
      Cmd.Parameters.Insert(0, param);
    }

    public void AddStringReturn(ushort maxChars)
    {
      AddStringReturn(DefaultParamName(), maxChars);
    }

    public void AddInputOutput(string paramName, double paramValue)
    {
      if (paramValue <= Double.MinValue || paramValue >= Double.MaxValue ||
           double.IsNaN(paramValue) || double.IsInfinity(paramValue))
      {
        SqlParameter param = new SqlParameter(paramName, SqlDbType.Float);
        param.Direction = ParameterDirection.Output;
        Cmd.Parameters.Add(param);
      }
      else
      {
        SqlParameter param = new SqlParameter(paramName, SqlDbType.Float);
        param.Direction = ParameterDirection.InputOutput;
        param.Value = paramValue;
        Cmd.Parameters.Add(param);
      }
    }

    public SqlParameterCollection Parameters
    {
      get { return Cmd.Parameters; }
    }

    private string DefaultParamName()
    {
      return string.Format("@{0}", Cmd.Parameters.Count.ToString());
    }

    public List<object> Outputs = new List<object>();
    public object ReturnValue = null;
    internal SqlCommand Cmd = new SqlCommand();
  }

}
