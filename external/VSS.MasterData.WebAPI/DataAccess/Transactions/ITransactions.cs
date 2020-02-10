using KafkaModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.Transactions
{
	public interface ITransactions
	{
		bool Execute(List<Action> actions);

		//TODO: Async version
		void Upsert<T>(T obj) where T : IDbTable;
		void Upsert<T>(IEnumerable<T> obj) where T : IDbTable;
		void Delete<T>(IEnumerable<T> obj) where T : IDbTable;
		void Delete(string query);

		void Publish(KafkaMessage kafkaMessage);
		void Publish(List<KafkaMessage> kafkaMessages);

		IEnumerable<T> Get<T>(string query, object parameters = null) where T : class;
		IEnumerable<T> GetWithTransaction<T>(string query, bool readUncommitted, object parameters = null) where T : class;
		Task<IEnumerable<T>> GetAsync<T>(string query, object parameters = null) where T : class;
		Task<List<Tuple<T1, T2>>> GetAsync<T1, T2>(string query, string splitOn, object parameters = null);
		Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> GetMultipleResultSetAsync<T1, T2>(string query, object parameters);
		List<Tuple<T1, T2>> Get<T1, T2>(string query, string splitOn, object parameters = null);
		List<Tuple<T1, T2, T3, T4>> Get<T1, T2, T3, T4>(string query, string splitOn, object parameters = null);
		object GetValue(string query);
	}
}
