using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using MySql.Data.MySqlClient;
using Polly;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Dapper;
using VSS.GenericConfiguration;

namespace Repositories
{
    public class RepositoryBase
    {
        private readonly string connectionString = string.Empty;
        private readonly ILogger log;

        protected MySqlConnection Connection = null;
        private bool isInTransaction;

        // this is used by the unit tests only 
        protected static int dbSyncRetryCount = 3;

        protected static int dbSyncMsDelay = 500;
        protected static int dbSyncRetryCountSoFar = 0;
        protected Policy dbSyncPolicy;

        protected static int dbAsyncRetryCount = 3;
        protected static int dbAsyncMsDelay = 500;
        protected static int dbAsyncRetriesSoFar = 0;
        protected Policy dbAsyncPolicy;



        protected RepositoryBase(IConfigurationStore _connectionString, ILoggerFactory logger)
        {
            this.connectionString = _connectionString.GetConnectionString("VSPDB");
            log = logger.CreateLogger<RepositoryBase>();
            dbAsyncPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(
                retryCount: dbAsyncRetryCount,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(dbAsyncMsDelay),
                onRetry: (exception, calculatedWaitDuration) =>
                {
                    log.LogError(
                        "Repository: Failed attempt to query/update db. Exception: {0}. Retries: {1}. RetryCountSoFar: {2}",
                        exception.Message, dbAsyncRetryCount, dbAsyncRetriesSoFar + 1);
                    dbAsyncRetriesSoFar++;
                });
            dbSyncPolicy = Policy.Handle<Exception>().WaitAndRetry(
                retryCount: dbSyncRetryCount,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(dbSyncMsDelay),
                onRetry: (exception, calculatedWaitDuration) =>
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
                var res =  await body(connection);
                connection.Close();
                return res;
            }
        }


        protected async Task<IEnumerable<T>> QueryWithAsyncPolicy<T>(string statement, object param = null)
        {
            if (!isInTransaction)
                return await dbAsyncPolicy.ExecuteAsync(async () =>
                    await WithConnectionAsync(async (conn) => await conn.QueryAsync<T>(statement, param)));
            return await Connection.QueryAsync<T>(statement, param);
        }

        protected async Task<int> ExecuteWithAsyncPolicy(string statement, object param = null)
        {
            if (!isInTransaction)
                return await dbAsyncPolicy.ExecuteAsync(async () =>
                    await WithConnectionAsync(async (conn) => await conn.ExecuteAsync(statement, param)));
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
                        await dbAsyncPolicy.ExecuteAsync(async () =>
                        {
                            await Connection.OpenAsync();
                            log.LogTrace("Repository: db opened successfully after {0} retries", dbAsyncRetriesSoFar);
                        });
                }
            }
        }


        public void Dispose()
        {
            PerhapsCloseConnection();
            Connection?.Dispose();
        }
    }
}