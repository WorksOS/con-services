using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.Data.Confluent;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.Transactions
{
	public class ExecuteTransaction : ITransactions
	{
		private readonly IDatabase _database;
		private readonly IPublisher _publisher;
		private readonly ILogger logger;
		public ExecuteTransaction(IConfiguration configuration, ILogger logger)
		{
			this.logger = logger;
			_database = new MySqlDatabase(configuration, logger);
			_publisher = new ConfluentPublisher(configuration, logger);
		}
		public bool Execute(List<Action> actions)
		{
			try
			{
				foreach (var action in actions)
				{
					action();
				}
				_database.Commit();
				_database.Dispose();
				return true;
			}
			catch (Exception ex)
			{
				logger.LogError($"vlmd_transaction_Error: Error while execute actions Error : {ex.Message + ex.StackTrace} ");
				_database.Rollback();
				_database.Dispose();
				throw;
			}
		}

		public void Delete<T>(IEnumerable<T> obj) where T : IDbTable
		{
			_database.Delete(obj);
		}

		public void Upsert<T>(IEnumerable<T> obj) where T : IDbTable
		{
			_database.Upsert(obj);
		}

		public void Publish(KafkaMessage kafkaMessage)
		{
			try
			{
				_publisher.Publish(kafkaMessage);
			}
			catch (Exception ex)
			{
				logger.LogError($"vlmd_transaction_Error:Error while publish Error : {ex.Message + ex.StackTrace} ");
				DisposePublisher();
				_publisher.RetryPublish(kafkaMessage);
			}
		}
		public void Publish(List<KafkaMessage> kafkaMessages)
		{
			try
			{
				_publisher.Publish(kafkaMessages);
			}
			catch (Exception ex)
			{
				logger.LogError($"vlmd_transaction_Error:Error while publish Error : {ex.Message + ex.StackTrace} ");
				DisposePublisher();
				_publisher.RetryPublish(kafkaMessages);
			}
			//using (profiler.Step("Publish Kafka Messages"))
			//{
			//	RetryPublisher(() => );
			//}
		}

		private void DisposePublisher()
		{
			lock (_publisher)
			{
				_publisher.Dispose();
			}
		}

		public IEnumerable<T> Get<T>(string query, object parameters) where T : class
		{
			return _database.Get<T>(query, parameters);
		}

		public async Task<IEnumerable<T>> GetAsync<T>(string query, object parameters) where T : class
		{
			var result = await _database.GetAsync<T>(query, parameters);
			return result;
		}

		public List<Tuple<T1, T2>> Get<T1, T2>(string query, string splitOn, object parameters)
		{
			return _database.Get<T1, T2>(query, splitOn, parameters);
		}

		public async Task<List<Tuple<T1, T2>>> GetAsync<T1, T2>(string query, string splitOn, object parameters)
		{
			var result = await _database.GetAsync<T1, T2>(query, splitOn, parameters);
			return result;
		}

		public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> GetMultipleResultSetAsync<T1, T2>(string query, object parameters)
		{
			var result = await _database.GetMultipleResultSetAsync<T1, T2>(query, parameters);
			return result;
		}

		public List<Tuple<T1, T2, T3, T4>> Get<T1, T2, T3, T4>(string query, string splitOn, object parameters)
		{
			return _database.Get<T1, T2, T3, T4>(query, splitOn, parameters);
		}

		public void Delete(string query)
		{
			_database.Delete(query);
		}

		private bool RetryPublisher(Action action)
		{
			var result = Policy.Handle<Exception>().Retry(3).ExecuteAndCapture(() => action);
			return !result.ExceptionType.HasValue;
		}

		private bool RetryDBQuery(Action action)
		{
			var result = Policy.Handle<DbException>().Retry(3).ExecuteAndCapture(() => action);
			return !result.ExceptionType.HasValue;
		}

		public void Upsert<T>(T obj) where T : IDbTable
		{
			_database.Upsert(new List<T> { obj });
			//RetryDBQuery(() => ));
		}
		public object GetValue(string query)
		{
			return _database.GetValue(query);
		}

		public IEnumerable<T> GetWithTransaction<T>(string query, bool readUncommitted,
													object parameters = null) where T : class
		{
			return _database.GetWithTransaction<T>(query, parameters, readUncommitted);
		}
	}
}
