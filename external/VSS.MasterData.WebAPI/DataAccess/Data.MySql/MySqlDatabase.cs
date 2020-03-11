using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.Data.MySql.Cache;
using VSS.MasterData.WebAPI.Data.MySql.Extensions;
using VSS.MasterData.WebAPI.Data.MySql.Helpers;

namespace VSS.MasterData.WebAPI.Data.MySql
{
	public sealed class MySqlDatabase : IDatabase
	{
		private readonly string connectionString;
		private IDbConnection connection;
		private IDbTransaction transaction;
		private static Dictionary<Type, List<string>> queryCache;
		private ILogger logger;


		public MySqlDatabase(IConfiguration configuration, ILogger logger)
		{
			this.logger = logger;
			connectionString = configuration["ConnectionString:MasterData"];
			queryCache = new Dictionary<Type, List<string>>();
			SqlMapper.AddTypeHandler(new MySqlGuidTypeHandler());
		}

		public void GetConnectionAndTransaction(bool readUncommitted = false)
		{
			if (connection == null)
			{
				connection = new MySqlConnection(connectionString);
			}

			if (connection.State == ConnectionState.Closed)
			{
				connection.Open();
			}

			if (transaction == null)
			{

				transaction = readUncommitted ? connection.BeginTransaction(IsolationLevel.ReadUncommitted) : connection.BeginTransaction();
			}
		}

		public MySqlConnection GetGetConnection()
		{

			var con = new MySqlConnection(connectionString);
			con.Open();
			return con;
		}

		public void Commit()
		{
			try
			{
				transaction?.Commit();
			}
			finally
			{
				transaction?.Dispose();
				transaction = null;

				connection?.Dispose();
				connection = null;
			}

		}

		public void Dispose()
		{
			try
			{
				transaction?.Dispose();
			}
			finally
			{
				transaction = null;

				connection?.Dispose();
				connection = null;
			}
		}
		public void Rollback()
		{
			try
			{
				transaction?.Rollback();
			}
			finally
			{
				transaction?.Dispose();
				transaction = null;

				connection?.Dispose();
				connection = null;
			}
		}

		public void Upsert<T>(IEnumerable<T> items) where T : IDbTable
		{
			var firstItem = items.FirstOrDefault();
			if (firstItem != null)
			{
				var columnList = GetOrBuildColumnList(firstItem);
				string[] objColumnList = columnList.Keys.ToArray();
				string[] dbColumnList = columnList.Values.ToArray();
				string[] ignoreColumns = firstItem.GetIgnoreColumnsOnUpdate().Split(',')?.ToArray();
				string tableName = firstItem.GetTableName();
				string[] dbColumnListWithIgnoreColumnList = GetColumnListWithIgnoreProperties(firstItem.GetType(), firstItem, ignoreColumns).ToArray();
				var builder = new StringBuilder();
				foreach (var item in items)
				{
					BuildValueQuery(item, objColumnList, ref builder);
				}
				MySqlFormatter.RemoveLastCharacter(builder);
				var upsertQuery = MySqlQuery.BuildUpsertQuery(tableName, dbColumnList, builder.ToString(), dbColumnListWithIgnoreColumnList);
				ExecuteNonQuery(upsertQuery);
			}
		}


		public void Delete<T>(IEnumerable<T> items) where T : IDbTable
		{
			var firstItem = items.FirstOrDefault();
			string keyColumn = firstItem.GetIdColumn();
			string tableName = firstItem.GetTableName();
			List<string> values = new List<string>();
			var builder = new StringBuilder();
			foreach (var item in items)
			{
				BuildValueQuery(item, new string[] { keyColumn }, ref builder);
			}
			MySqlFormatter.RemoveLastCharacter(builder);
			var deleteQuery = MySqlQuery.BuildDeleteQuery(tableName, keyColumn, builder.ToString());
			ExecuteNonQuery(deleteQuery);
		}

		public IEnumerable<T> Get<T>(string query, object parameters) where T : class
		{
			using (var con = GetGetConnection())
			{

				return con.Query<T>(query, parameters);
			}

		}

		public async Task<IEnumerable<T>> GetAsync<T>(string query, object parameters) where T : class
		{
			using (var con = GetGetConnection())
			{
				return await con.QueryAsync<T>(query, parameters, commandTimeout: 60);

			}

		}

		public List<Tuple<T1, T2>> Get<T1, T2>(string query, string splitOn, object parameters)
		{
			using (var con = GetGetConnection())
			{
				return con
					.Query<T1, T2, Tuple<T1, T2>>(query, Tuple.Create, parameters, splitOn: splitOn,
						commandType: CommandType.Text)
					.AsList();
			}

		}

		public async Task<List<Tuple<T1, T2>>> GetAsync<T1, T2>(string query, string splitOn, object parameters)
		{
			using (var con = GetGetConnection())
			{
				var result = await con
					.QueryAsync<T1, T2, Tuple<T1, T2>>(query, Tuple.Create, parameters, splitOn: splitOn,
						commandType: CommandType.Text);
				return result.ToList();
			}


		}

		public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> GetMultipleResultSetAsync<T1, T2>(string query, object parameters)
		{
			using (var con = GetGetConnection())
			{
				Tuple<IEnumerable<T1>, IEnumerable<T2>> result = null;
				var reader = await con
					.QueryMultipleAsync(query, parameters);
				while (!reader.IsConsumed)
				{
					var TOut1 = reader.Read<T1>();
					var TOut2 = reader.Read<T2>();
					result = Tuple.Create(TOut1, TOut2);
				}

				return result;
			}

		}

		public List<Tuple<T1, T2, T3, T4>> Get<T1, T2, T3, T4>(string query, string splitOn, object parameters)
		{
			using (var con = GetGetConnection())
			{
				return con
					.Query<T1, T2, T3, T4, Tuple<T1, T2, T3, T4>>(query, Tuple.Create, parameters, splitOn: splitOn, commandType: CommandType.Text)
					.AsList();
			}

		}

		public void Delete(string query)
		{
			using (var con = GetGetConnection())
			{
				con.Execute(query);
			}

		}

		private Dictionary<string, string> GetOrBuildColumnList<T>(T tObject) where T : IDbTable
		{
			var tType = tObject.GetType();
			if (!CacheEntry.TypeCache.TryGetValue(tType, out var columns))
			{
				columns = GetColumnsList(tObject, tType);
			}
			return columns;
		}

		private Dictionary<string, string> GetColumnsList<T>(T tObject, Type type) where T : IDbTable
		{
			Dictionary<string, string> columnList;
			columnList = tObject.GetTableNameAndColumns<T>();
			if (columnList == null || columnList.Count <= 0)
			{
				columnList = type.GetProperties().Select(x => x.Name).ToDictionary(x => x, x => x);
			}
			CacheEntry.TypeCache.TryAdd(type, columnList);
			return columnList;
		}

		private List<string> GetColumnListWithIgnoreProperties<T>(Type type, T tObject, string[] ignoreColumns) where T : IDbTable
		{
			if (!CacheEntry.TypeCache.TryGetValue(type, out var columns))
			{
				columns = GetColumnsList(tObject, type);
			}

			return ignoreColumns?.Length > 0 ? columns.Values.Except(ignoreColumns).ToList() : columns.Values.ToList();
		}

		private void BuildValueQuery<T>(T value, string[] dbColumnList, ref StringBuilder queryBuilder)
			where T : IDbTable
		{
			queryBuilder.Append("(");

			foreach (var column in dbColumnList)
			{
				var propertyValue = value.GetType().GetProperty(column)?.GetValue(value, null);
				queryBuilder.Append(MySqlFormatter.ConvertValueToMySqlFormatstring(propertyValue));
				queryBuilder.Append(",");
			}

			MySqlFormatter.RemoveLastCharacter(queryBuilder);
			queryBuilder.Append("),");
		}


		private void ExecuteNonQuery(string sql)
		{
			try
			{
				using (IDbCommand dbCommand = CreateCommand(sql))
				{
					dbCommand.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"vlmd_database_error: SQL: {sql} Error: {ex.Message + ex.StackTrace} ");
				throw;
			}
		}

		private IDbCommand CreateCommand(string sql)
		{
			GetConnectionAndTransaction();
			IDbCommand dbCommand = connection.CreateCommand();
			dbCommand.CommandText = sql;

			return dbCommand;
		}

		public object GetValue(string query)
		{

			using (var con = GetGetConnection())
			{
				var result = con.ExecuteScalar(query);
				return result;
			}

		}

		public IEnumerable<T> GetWithTransaction<T>(string query, object parameters,
			bool readUncommitted) where T : class
		{

			GetConnectionAndTransaction(readUncommitted);

			var result = connection.Query<T>(query, parameters);

			return result;
		}
	}
}