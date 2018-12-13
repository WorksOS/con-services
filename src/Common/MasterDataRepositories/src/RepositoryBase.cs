using Dapper;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Polly;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VSS.ConfigurationStore;

namespace VSS.MasterData.Repositories
{
  public abstract class RepositoryBase
  {
    protected ILogger log;

    // this is used by the unit tests only 
    private const int dbSyncRetryCount = 3;
    private const int dbSyncMsDelay = 500;

    private const int dbAsyncRetryCount = 3;
    private const int dbAsyncMsDelay = 500;

    private readonly string connectionString = string.Empty;

    private MySqlConnection Connection;
    private bool isInTransaction;


    protected RepositoryBase(IConfigurationStore configurationStore, ILoggerFactory logger)
    {
      connectionString = configurationStore.GetConnectionString("VSPDB");
      log = logger.CreateLogger<RepositoryBase>();
    }


    private T WithConnection<T>(Func<MySqlConnection, T> body)
    {
      T result = default(T);
      log.LogDebug($"Repository PollySync: going to execute sync policy");

      var policyResult = Policy
        .Handle<Exception>()
        .WaitAndRetry(
          dbSyncRetryCount,
          attempt => TimeSpan.FromMilliseconds(dbSyncMsDelay),
          (exception, calculatedWaitDuration) =>
          {
            log.LogError(
              $"Repository: Failed attempt to query/update db. Exception: {exception.Message}. Retries: {dbSyncRetryCount}.");
          })
        .ExecuteAndCapture(() =>
        {
          using (var connection = new MySqlConnection(connectionString))
          {
            connection.Open();
            log.LogTrace("Repository: db open (with connection reuse) was successfull");
            result = body(connection);
            connection.Close();
            return result;
          }
        });

      if (policyResult.FinalException != null)
      {
        log.LogCritical(
          $"Repository {this.GetType().FullName} failed with exception {policyResult.FinalException.ToString()}");
        throw policyResult.FinalException;
      }

      return result;
    }


    private async Task<T> WithConnectionAsync<T>(Func<MySqlConnection, Task<T>> body)
    {
      T result = default(T);
      log.LogDebug($"Repository PollyAsync: going to execute async policy");

      var policyResult = await Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
          dbAsyncRetryCount,
          attempt => TimeSpan.FromMilliseconds(dbAsyncMsDelay),
          (exception, calculatedWaitDuration) =>
          {
            log.LogError(
              $"Repository: Failed attempt to query/update db. Exception: {exception.Message}. Retries: {dbAsyncRetryCount}.");
          })
        .ExecuteAndCaptureAsync(async () =>
        {
          using (var connection = new MySqlConnection(connectionString))
          {
            await connection.OpenAsync();
            log.LogTrace("Repository: db open (with connection reuse) was successfull");
            result = await body(connection);
            connection.Close();
            return result;
          }
        });

      if (policyResult.FinalException != null)
      {
        log.LogCritical(
          $"Repository {this.GetType().FullName} failed with exception {policyResult.FinalException.ToString()}");
        throw policyResult.FinalException;
      }

      return result;
    }


    protected async Task<IEnumerable<T>>
      QueryWithAsyncPolicy<T>(string statement, object param = null)
    {
      if (!isInTransaction)
        return await WithConnectionAsync(async conn => await conn.QueryAsync<T>(statement, param));
      return await Connection.QueryAsync<T>(statement, param);
    }

    protected async Task<int> ExecuteWithAsyncPolicy(string statement, object param = null)
    {
      if (!isInTransaction)
        return await WithConnectionAsync(async conn => await conn.ExecuteAsync(statement, param));
      return await Connection.ExecuteAsync(statement, param);
    }


    //For unit tests
    public async Task<T> InRollbackTransactionAsync<T>(Func<object, Task<T>> body)
    {
      return await WithConnectionAsync(async conn =>
      {
        MySqlTransaction transaction = null;

        try
        {
          isInTransaction = true;
          transaction = conn.BeginTransaction();
          Connection = conn;
          var result = await body(conn);
          return result;
        }
        finally
        {
          if (transaction != null && Connection.State == ConnectionState.Open)
            transaction.Rollback();
          isInTransaction = false;
        }
      });
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
          isInTransaction = true;
          transaction = conn.BeginTransaction();
          Connection = conn;
          var result = body(conn);
          return result;
        }
        finally
        {
          if (transaction != null && Connection.State == ConnectionState.Open)
            transaction.Rollback();
          isInTransaction = false;
        }
      });
    }

    public void Dispose()
    {
      Connection?.Dispose();
    }
  }
}