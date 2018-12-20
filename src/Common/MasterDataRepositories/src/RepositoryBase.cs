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
    protected ILogger Log;

    // this is used by the unit tests only 
    private const int DbSyncRetryCount = 3;
    private const int DbSyncMsDelay = 500;

    private const int DbAsyncRetryCount = 3;
    private const int DbAsyncMsDelay = 500;

    private readonly string _connectionString = string.Empty;

    private MySqlConnection _connection;
    private bool _isInTransaction;


    protected RepositoryBase(IConfigurationStore configurationStore, ILoggerFactory logger)
    {
      _connectionString = configurationStore.GetConnectionString("VSPDB");
      Log = logger.CreateLogger<RepositoryBase>();
    }


    private T WithConnection<T>(Func<MySqlConnection, T> body)
    {
      T result = default(T);

      var policyResult = Policy
        .Handle<Exception>()
        .WaitAndRetry(
          DbSyncRetryCount,
          attempt => TimeSpan.FromMilliseconds(DbSyncMsDelay),
          (exception, calculatedWaitDuration) =>
          {
            Log.LogError(
              $"Repository PollySync: Failed attempt to query/update db. Exception: {exception.Message}. Retries: {DbSyncRetryCount}.");
          })
        .ExecuteAndCapture(() =>
        {
          using (var connection = new MySqlConnection(_connectionString))
          {
            connection.Open();
            Log.LogTrace("Repository PollySync: db open (with connection reuse) was successfull");
            result = body(connection);
            connection.Close();
            return result;
          }
        });

      if (policyResult.FinalException != null)
      {
        Log.LogCritical(
          $"Repository PollySync: {this.GetType().FullName} failed with exception: ", policyResult.FinalException);
        throw policyResult.FinalException;
      }

      return result;
    }


    private async Task<T> WithConnectionAsync<T>(Func<MySqlConnection, Task<T>> body)
    {
      T result = default(T);
      Log.LogDebug("Repository PollyAsync: going to execute async policy");

      var policyResult = await Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
          DbAsyncRetryCount,
          attempt => TimeSpan.FromMilliseconds(DbAsyncMsDelay),
          (exception, calculatedWaitDuration) =>
          {
            Log.LogError(
              $"Repository PollyAsync: Failed attempt to query/update db. Exception: {exception.Message}. Retries: {DbAsyncRetryCount}.");
          })
        .ExecuteAndCaptureAsync(async () =>
        {
          using (var connection = new MySqlConnection(_connectionString))
          {
            await connection.OpenAsync();
            Log.LogTrace("Repository PollyAsync: db open (with connection reuse) was successfull");
            result = await body(connection);
            connection.Close();
            return result;
          }
        });

      if (policyResult.FinalException != null)
      {
        Log.LogCritical(
          $"Repository PollyAsync: {this.GetType().FullName} failed with exception: ", policyResult.FinalException);
        throw policyResult.FinalException;
      }

      return result;
    }


    protected async Task<IEnumerable<T>>
      QueryWithAsyncPolicy<T>(string statement, object param = null)
    {
      if (!_isInTransaction)
        return await WithConnectionAsync(async conn => await conn.QueryAsync<T>(statement, param));
      return await _connection.QueryAsync<T>(statement, param);
    }

    protected async Task<int> ExecuteWithAsyncPolicy(string statement, object param = null)
    {
      if (!_isInTransaction)
        return await WithConnectionAsync(async conn => await conn.ExecuteAsync(statement, param));
      return await _connection.ExecuteAsync(statement, param);
    }


    //For unit tests
    public async Task<T> InRollbackTransactionAsync<T>(Func<object, Task<T>> body)
    {
      return await WithConnectionAsync(async conn =>
      {
        MySqlTransaction transaction = null;

        try
        {
          _isInTransaction = true;
          transaction = conn.BeginTransaction();
          _connection = conn;
          var result = await body(conn);
          return result;
        }
        finally
        {
          if (transaction != null && _connection.State == ConnectionState.Open)
            transaction.Rollback();
          _isInTransaction = false;
        }
      });
    }

    //For unit tests
    public T InRollbackTransaction<T>(Func<object, T> body)
    {
      _isInTransaction = true;
      return WithConnection(conn =>
      {
        MySqlTransaction transaction = null;

        try
        {
          _isInTransaction = true;
          transaction = conn.BeginTransaction();
          _connection = conn;
          var result = body(conn);
          return result;
        }
        finally
        {
          if (transaction != null && _connection.State == ConnectionState.Open)
            transaction.Rollback();
          _isInTransaction = false;
        }
      });
    }

    public void Dispose()
    {
      _connection?.Dispose();
    }
  }
}