using System;
using System.Configuration;
using System.Data;
using System.Reflection;
using log4net;
using MySql.Data.MySqlClient;

namespace LandfillService.Common.Repositories
{
  public class RepositoryBase
  {
    protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    protected readonly string connectionString;

    protected MySqlConnection Connection;
    private bool isInTransaction;

    protected RepositoryBase()
    {
      connectionString = ConfigurationManager.ConnectionStrings["MySql.Connection"].ConnectionString;
    }

    public void SetInTransactionState(bool value)
    {
      isInTransaction = value;
    }

    private T WithConnection<T>(Func<MySqlConnection, T> body)
    {
      using (var connection = new MySqlConnection(connectionString))
      {
        connection.Open();
        Connection = connection;
        var res = body(connection);
        connection.Close();
        return res;
      }
    }

    //For unit tests
    public T InRollbackTransaction<T>(Func<object, T> body)
    {
      isInTransaction = true;
      return WithConnection(conn =>
      {
        MySqlTransaction transaction = null;

        try
        {
          transaction = conn.BeginTransaction();
          Connection = conn;
          var result = body(conn);
          return result;
        }
        finally
        {
          if (transaction != null)
            transaction.Rollback();
        }
      });
    }

    protected void PerhapsCloseConnection()
    {
      if (!isInTransaction)
      {
        if (Connection != null && Connection.State != ConnectionState.Closed)
        {
          Connection.Close();
        }
      }
    }

    protected void PerhapsOpenConnection()
    {
      if (!isInTransaction)
      {
        if (Connection == null || Connection.State == ConnectionState.Closed)
        {
          Connection = new MySqlConnection(connectionString);
        }
        if (Connection != null && Connection.State == ConnectionState.Closed)
        {
          Connection.Open();
        }
      }
    }
  }
}
