using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.Data.MySql
{
	public interface IDatabase
	{
		void Upsert<T>(IEnumerable<T> obj) where T : IDbTable;
		IEnumerable<T> Get<T>(string query, object parameters) where T : class;
		IEnumerable<T> GetWithTransaction<T>(string query, object parameters, bool readUncommitted) where T : class;
		Task<IEnumerable<T>> GetAsync<T>(string query, object parameters) where T : class;
		Task<List<Tuple<T1, T2>>> GetAsync<T1, T2>(string query, string splitOn, object parameters);
		List<Tuple<T1, T2>> Get<T1, T2>(string query, string splitOn, object parameters);
		List<Tuple<T1, T2, T3, T4>> Get<T1, T2, T3, T4>(string query, string splitOn, object parameters);
		Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> GetMultipleResultSetAsync<T1, T2>(string query, object parameters);
		object GetValue(string query);
		void Delete<T>(IEnumerable<T> items) where T : IDbTable;
		void Delete(string query);
		void Commit();
		void Dispose();
		void Rollback();
	}
}
