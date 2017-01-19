using System;
using System.Data;
using System.Reflection;

using MySql.Data.MySqlClient;
using Polly;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using VSS.Project.Service.Utils;

namespace VSS.Project.Service.Repositories
{
  public class RepositoryBase 
  {
    private readonly string connectionString=string.Empty;
    //private readonly ILogger log;

    protected MySqlConnection Connection=null;
    private bool isInTransaction;

    // this is used by the unit tests only 
    protected static int dbSyncRetryCount = 3;
    protected static int dbSyncMsDelay = 500;
    protected static int dbSyncRetryCountSoFar = 0;
    protected Policy dbSyncPolicy = Policy.Handle<Exception>().WaitAndRetry(
          retryCount: dbSyncRetryCount,
          sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(dbSyncMsDelay), 
          onRetry: (exception, calculatedWaitDuration) =>
          {
            //log.LogInformation("Repository: Failed attempt to open db. Exception: {0}. Retries: {1}. RetryCountSoFar: {2}", exception.Message, dbSyncRetryCount, dbSyncRetryCountSoFar + 1);
            dbSyncRetryCountSoFar++;
          });

    protected static int dbAsyncRetryCount = 3;
    protected static int dbAsyncMsDelay = 500;
    protected static int dbAsyncRetriesSoFar = 0;
    protected Policy dbAsyncPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(
          retryCount: dbAsyncRetryCount,
          sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(dbAsyncMsDelay),
          onRetry: (exception, calculatedWaitDuration) => 
          {
            //log.LogDebug("Repository: Failed attempt to query/update db. Exception: {0}. Retries: {1}. RetryCountSoFar: {2}", exception.Message, dbAsyncRetryCount, dbAsyncRetriesSoFar + 1);
            dbAsyncRetriesSoFar++;
          });



    protected RepositoryBase(IConfigurationStore _connectionString)
    {
      this.connectionString = _connectionString.GetConnectionString("VSPDB");
      //log = logger.CreateLogger <RepositoryBase>();
    }



    private T WithConnection<T>(Func<MySqlConnection, T> body)
    {
      using (var connection = new MySqlConnection(connectionString))
      {
        try
        {
          dbSyncRetryCountSoFar = 0;
          dbSyncPolicy.Execute(() =>
          {
            connection.Open();
            //log.LogDebug("Repository: db open (with connection reuse) was successfully after {0} retries", dbSyncRetryCountSoFar);
          });
        }
        catch (Exception e)
        {
          //log.LogInformation("Repository: db open (with connection reuse). Retries so far {0}.", dbSyncRetryCountSoFar);
        }
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
          if (transaction != null && Connection.State == ConnectionState.Open)
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
          Connection.Dispose();
        }
      }
    }

    protected async Task PerhapsOpenConnection()
    {
      if (!isInTransaction)
      {
        if (Connection == null)
        {
          Connection = new MySqlConnection(connectionString);
        }
        if (Connection != null && Connection.State == ConnectionState.Closed)
        {
          try
          {
            await dbAsyncPolicy.ExecuteAsync(async () =>
            {
              await Connection.OpenAsync();
              //log.LogInformation("Repository: db opened successfully after {0} retries", dbAsyncRetriesSoFar);
            });
          }
          catch (Exception e)
          {
            //log.LogInformation("Repository: db open eventually failed with: {0}. after {1} retries.", e.Message, dbAsyncRetriesSoFar);
          }
        }
      }
    }


    public void Dispose()
    {
      PerhapsCloseConnection();
      if (Connection!=null)
        Connection.Dispose();
    }
  }
}