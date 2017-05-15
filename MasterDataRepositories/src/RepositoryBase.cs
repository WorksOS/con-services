using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Polly;
using VSS.GenericConfiguration;

namespace Repositories
{
    public class RepositoryBase
    {
        // this is used by the unit tests only 
        private static readonly int dbSyncRetryCount = 3;

        private static readonly int dbSyncMsDelay = 500;
        private static int dbSyncRetryCountSoFar;

        private static readonly int dbAsyncRetryCount = 3;
        private static readonly int dbAsyncMsDelay = 500;
        private static int dbAsyncRetriesSoFar;
        private readonly string connectionString = string.Empty;
        private readonly ILogger log;

        private MySqlConnection Connection;
        private readonly Policy dbAsyncPolicy;
        private Policy dbSyncPolicy;
        private bool isInTransaction;


        protected RepositoryBase(IConfigurationStore _connectionString, ILoggerFactory logger)
        {
            connectionString = _connectionString.GetConnectionString("VSPDB");
            log = logger.CreateLogger<RepositoryBase>();
            dbAsyncPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(
                dbAsyncRetryCount,
                attempt => TimeSpan.FromMilliseconds(dbAsyncMsDelay),
                (exception, calculatedWaitDuration) =>
                {
                    log.LogError(
                        "Repository: Failed attempt to query/update db. Exception: {0}. Retries: {1}. RetryCountSoFar: {2}",
                        exception.Message, dbAsyncRetryCount, dbAsyncRetriesSoFar + 1);
                    dbAsyncRetriesSoFar++;
                });
            dbSyncPolicy = Policy.Handle<Exception>().WaitAndRetry(
                dbSyncRetryCount,
                attempt => TimeSpan.FromMilliseconds(dbSyncMsDelay),
                (exception, calculatedWaitDuration) =>
                {
                    log.LogError(
                        "Repository: Failed attempt to query/update db. Exception: {0}. Retries: {1}. RetryCountSoFar: {2}",
                        exception.Message, dbSyncRetryCount, dbSyncRetryCountSoFar + 1);
                    dbSyncRetryCountSoFar++;
                });
        }

        private T WithConnection<T>(Func<MySqlConnection, T> body)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                dbAsyncPolicy.Execute(() =>
                {
                    connection.Open();
                    log.LogTrace("Repository: db open (with connection reuse) was successfull");
                });
                var res = body(connection);
                connection.Close();
                return res;
            }
        }

        private async Task<T> WithConnectionAsync<T>(Func<MySqlConnection, Task<T>> body)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await dbAsyncPolicy.ExecuteAsync(async () =>
                {
                    await connection.OpenAsync();
                    log.LogTrace("Repository: db open (with connection reuse) was successfull");
                });
                var res = await body(connection);
                connection.Close();
                return res;
            }
        }


        protected async Task<IEnumerable<T>> QueryWithAsyncPolicy<T>(string statement, object param = null)
        {
            if (!isInTransaction)
                await WithConnectionAsync(async conn => await conn.QueryAsync<T>(statement, param));
            return await QueryWithAsyncPolicy<T>(statement, param);
        }

        protected async Task<int> ExecuteWithAsyncPolicy(string statement, object param = null)
        {
            if (!isInTransaction)
                await WithConnectionAsync(async conn => await conn.ExecuteAsync(statement, param));
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