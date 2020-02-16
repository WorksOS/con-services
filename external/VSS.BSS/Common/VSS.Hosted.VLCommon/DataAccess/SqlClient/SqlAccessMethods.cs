using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using log4net;
using System.Reflection;
using Microsoft.SqlServer.Server;
using System.Data.Entity.Core.EntityClient;

namespace VSS.Hosted.VLCommon
{
  public class SqlAccessMethods
  {
    private static readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    #region Public Interface
    public static SqlConnection GetConnection(string dbName)
    {
      return new SqlConnection(ObjectContextFactory.DbProviderConnectionString(dbName));
    }

    public static List<SqlDataRecord> Fill(IEnumerable<long> items, string itemName = "AssetID")
    {
      List<SqlDataRecord> result = null;

      if (null == items)
        return result;

      foreach (long item in items)
      {
        SqlDataRecord record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData(itemName, SqlDbType.BigInt) });
        record.SetInt64(0, item);

        if (null == result)
          result = new List<SqlDataRecord>();
        result.Add(record);
      }

      return result;
    }

    public static DataSet Fill(StoredProcDefinition def)
    {
      DataSet ds = new DataSet();
      Fill(def, ds);
      return ds;
    }

    public static void Fill(StoredProcDefinition def, DataSet ds)
    {
      using (SqlConnection cn = SqlAccessMethods.GetConnection(def.DbName))
      {
        cn.Open();
        def.Cmd.Connection = cn;
        SqlDataAdapter dataAdapter = new SqlDataAdapter();
        dataAdapter.SelectCommand = def.Cmd;
        dataAdapter.Fill(ds);
      }

      SetOutputs(def);
    }

    public static void Fill(StoredProcDefinition def, DataTable dt)
    {
      using (SqlConnection cn = SqlAccessMethods.GetConnection(def.DbName))
      {
        cn.Open();
        def.Cmd.Connection = cn;
        SqlDataAdapter dataAdapter = new SqlDataAdapter();
        dataAdapter.SelectCommand = def.Cmd;
        dataAdapter.Fill(dt);
      }

      SetOutputs(def);
    }

    public static object ExecuteScalar(StoredProcDefinition def)
    {
      object result = null;
      using (SqlConnection cn = SqlAccessMethods.GetConnection(def.DbName))
      {
        cn.Open();
        def.Cmd.Connection = cn;
        result = def.Cmd.ExecuteScalar();
      }
      return result;
    }

    public static SqlDataReader ExecuteReader(StoredProcDefinition def)
    {
      // Can't wrap the connection with a:
      //
      // using ( SqlConnection cn = SqlAccessMethods.GetConnection(def.DbName) )
      //
      // because the reader gets trashed when you exit the using scopem and
      // then it can't be iterated.

      SqlConnection cn = SqlAccessMethods.GetConnection(def.DbName);
      cn.Open();

      def.Cmd.Connection = cn;
      SqlDataReader reader = def.Cmd.ExecuteReader(CommandBehavior.CloseConnection);
      SetOutputs(def);
      return reader;
    }

    public static int ExecuteReaderForMerge(StoredProcDefinition procDefinition)
    {
      int insertCount = 0;
      int updateCount = 0;
      int otherCount = 0;

      using (var reader = SqlAccessMethods.ExecuteReader(procDefinition))
      {
        int actionIndex = reader.GetOrdinal("$action");
        while (reader.Read())
        {
          string action = reader.GetString(actionIndex);
          if ("INSERT".Equals(action, StringComparison.Ordinal))
          {
            insertCount++;
          }
          else if ("UPDATE".Equals(action, StringComparison.Ordinal))
          {
            updateCount++;
          }
          else
          {
            otherCount++;
          }
        }
      }

      return insertCount + updateCount;
    }

    public static int ExecuteNonQuery(StoredProcDefinition def)
    {
      using (SqlConnection conn = SqlAccessMethods.GetConnection(def.DbName))
      {
        conn.Open();
        return ExecuteNonQuery(def, conn);
      }
    }

    public static int ExecuteNonQueryWithPrepare(StoredProcDefinition def)
    {
      using (SqlConnection conn = SqlAccessMethods.GetConnection(def.DbName))
      {
        conn.Open();
        return ExecuteNonQueryWithPrepare(def, conn);
      }
    }

    public static int ExecuteNonQueryWithPrepare(StoredProcDefinition def, SqlConnection cn)
    {
      int rowsAffected = ExecuteNonQueryWithPrepare(def.Cmd, cn);

      SetOutputs(def);
      return rowsAffected;
    }

    public static int ExecuteNonQuery(StoredProcDefinition def, SqlConnection cn)
    {
      int rowsAffected = ExecuteNonQuery(def.Cmd, cn);

      SetOutputs(def);
      return rowsAffected;
    }

    public static int ExecuteNonQuery(StoredProcDefinition def, out long result)
    {
      result = -1;

      int execResult = SqlAccessMethods.ExecuteNonQuery(def);

      if (def.Outputs.Count == 1 && def.Outputs[0] != DBNull.Value)
      {
        result = long.Parse(def.Outputs[0].ToString());
      }

      return execResult;
    }

    public static int ExecuteNonQuery(StoredProcDefinition def, out bool result)
    {
      result = false;

      int execResult = SqlAccessMethods.ExecuteNonQuery(def);

      if (def.Outputs.Count == 1 && def.Outputs[0] != DBNull.Value)
      {
        result = int.Parse(def.Outputs[0].ToString()) == 1 ? true : false;
      }

      return execResult;
    }

    /// <summary>
    /// This method takes a StoredProcDefinition to execute against the provided connection if and only if that connection has an open transaction and is an entity connection type
    /// </summary>
    /// <param name="sp">The Stored Proc definition to be executed</param>
    /// <param name="connection">the connection to be used if it is an EntityConnection Type and has an open transaction 
    /// other wise it creates a new connection this is done to keep the old code working as is</param>
    /// <returns>number of rows affected</returns>
    public static int ExecuteNonQueryWithTransactionFromEntityConnection(StoredProcDefinition sp, EntityConnection connection)
    {
      //in order to keep things working the same as they were before if the transaction is null use a completely new connection
      SqlConnection conn = connection.StoreConnection as SqlConnection; 
      
      SqlTransaction transaction = SqlAccessMethods.GetTransaction(conn);
      if (transaction != null)
      {
        sp.Cmd.Transaction = transaction;
        return ExecuteNonQuery(sp, conn);
      }
      
      return ExecuteNonQuery(sp);
    }

    // See http://technet.microsoft.com/en-us/library/cc645603.aspx for full list, with descriptions
    private enum SqlErrorNumber
    {
      ClientTimeout = -2,
      ConnectionError1 = -1,
      ConnectionError2 = 2,
      ConnectionError3 = 53,
      NoNullAllowed = 515,
      PrimaryKeyViolation = 2627,
      DuplicateKeyViolation = 2601,
      RecordExists = 50501,
      RecordDoesNotExist = 50502,
      InsufficientPermission = 50503,
      UnknownLanguage = 50509,
      NoNullColumn = 50510,
      InvalidOperation = 50511,
      ParameterOutOfRange = 50512,
      RecordIDDoesNotExist = 50513,
      DuplicateValues = 50517,
      UnknownSecurityToken = 60005,
      //http://www.reflectionit.nl/DAL.aspx
      InvalidDatabase = 4060,
      LoginFailed = 18456,
      ForeignKeyViolation = 547,
      //http://staff.newtelligence.net/clemensv/CategoryView,category,Architecture,SOA.aspx
      LowMemoryCondition = 8651,
      TimeoutWaitingForMemory = 8645,
      LockRequestTimeout = 1222,
      DeadLockVictim = 1205,
      LockIssue = 1204,
      OutOfMemory = 701,
      TypeConversionError = 8114,
      InvalidColumnName = 207,

    };

    public static bool IsRetryableException(Exception ex)
    {
      bool retryable = false;

      SqlException sqlEx = ex as SqlException;
      if (sqlEx != null)
      {
        SqlErrorNumber sqlError = (SqlErrorNumber)sqlEx.Number;

        switch (sqlError)
        {
          // These are temporary issues, they will be fixed at some point, lets try again
          // note that some (e.g. deadlock) may leave the transaction and/or connection in an unknown
          // state, so 'finally' must clean these up so that a fresh start can be made.
          case SqlErrorNumber.InvalidDatabase:
          case SqlErrorNumber.LoginFailed:
          case SqlErrorNumber.LowMemoryCondition:
          case SqlErrorNumber.TimeoutWaitingForMemory:
          case SqlErrorNumber.LockRequestTimeout:
          case SqlErrorNumber.DeadLockVictim:
          case SqlErrorNumber.LockIssue:
          case SqlErrorNumber.ClientTimeout:
            retryable = true;
            break;

          default:
            break;
        }
      }

      //this could be a connection pool problem or some other timeout other than ClientTimeout but all timeouts should be retryable
      if (!string.IsNullOrEmpty(ex.Message) && ex.Message.Contains("Timeout"))
        retryable = true;

      return retryable;
    }
    #endregion

    #region Internals
    static private void SetOutputs(StoredProcDefinition def)
    {
      foreach (SqlParameter param in def.Cmd.Parameters)
      {
        if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
        {
          def.Outputs.Add(param.Value);
        }
        else if (param.Direction == ParameterDirection.ReturnValue)
        {
          def.ReturnValue = param.Value;
        }
      }
    }

    private static readonly PropertyInfo ConnectionInfo = typeof(SqlConnection).GetProperty("InnerConnection", BindingFlags.NonPublic | BindingFlags.Instance);
    /// <summary>
    /// Get the currently opened transaction if from the sqlconnection object
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    private static SqlTransaction GetTransaction(SqlConnection conn)
    {
      if (conn != null && conn.State != ConnectionState.Closed)
      {
        var internalConn = ConnectionInfo.GetValue(conn, null);
        var currentTransactionProperty = internalConn.GetType().GetProperty("CurrentTransaction", BindingFlags.NonPublic | BindingFlags.Instance);
        if (currentTransactionProperty != null)
        {
          var currentTransaction = currentTransactionProperty.GetValue(internalConn, null);
          if (currentTransaction != null)
          {
            var realTransactionProperty = currentTransaction.GetType().GetProperty("Parent", BindingFlags.NonPublic | BindingFlags.Instance);
            var realTransaction = realTransactionProperty.GetValue(currentTransaction, null);
            return realTransaction as SqlTransaction;
          }
        }
      }
      return null;
    }

    private static int ExecuteNonQuery(SqlCommand cmd, SqlConnection cn)
    {
      cmd.Connection = cn;
      return cmd.ExecuteNonQuery();
    }

    private static int ExecuteNonQueryWithPrepare(SqlCommand cmd, SqlConnection cn)
    {
      cmd.Connection = cn;
      cmd.Prepare();
      return cmd.ExecuteNonQuery();
    }

    #endregion
  }
}
