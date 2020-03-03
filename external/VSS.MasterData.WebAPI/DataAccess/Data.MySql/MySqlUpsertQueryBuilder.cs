//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using VSS.MasterData.WebAPI.Data.MySql.Cache;
//using VSS.MasterData.WebAPI.Data.MySql.Extensions;

//namespace VSS.MasterData.WebAPI.Data.MySql
//{
//	public class MySqlUpsertQueryBuilder : IQueryBuilder
//	{
//		private const string query = "INSERT INTO {0} ({1}) VALUES {2} ON DUPLICATE KEY UPDATE {3}";

//		public string Build<T>(List<T> values) where T:class
//		{
//			var tableNameColumns = CacheEntry.TypeCache.GetTableNameAndColumns(typeof(T));
//			var upsertColumnString = this.BuilUpsertColumnsString(tableNameColumns.Value);
//			var valuesString = this.BuildValuesString<T>(values, tableNameColumns.Value);
//			return string.Format(query, tableNameColumns.Key, valuesString, string.Join(",", tableNameColumns.Value), upsertColumnString);
//		}

//		private string BuilUpsertColumnsString(List<string> columns)
//		{
//			StringBuilder columnString = new StringBuilder();
//			foreach (string propertyName in columns)
//			{
//				columnString.Append($"{propertyName}=VALUES({propertyName}),");
//			}
//			MySqlFormatter.RemoveLastCharacter(columnString);
//			return columnString.ToString();
//		}

//		private string BuildValuesString<T>(List<T> values, List<string> columns)
//			where T : class
//		{
//			StringBuilder queryBuilder = new StringBuilder();
//			foreach (var value in values)
//			{
//				queryBuilder.Append("(");
//				foreach (var column in columns)
//				{
//					var propertyValue = value.GetType().GetProperty(column)?.GetValue(value, null);
//					queryBuilder.Append(MySqlFormatter.ConvertValueToMySqlFormatstring(propertyValue));
//					queryBuilder.Append(",");
//				}
//				MySqlFormatter.RemoveLastCharacter(queryBuilder);
//				queryBuilder.Append("),");
//			}
//			MySqlFormatter.RemoveLastCharacter(queryBuilder);
//			return queryBuilder.ToString();
//		}
//	}
//}

