using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.Data.MySql
{
	public class MySqlQuery
	{
		private const string upsertQuery = "INSERT INTO {0} ({1}) VALUES {2} ON DUPLICATE KEY UPDATE {3}";

		private const string deleteQuery = "DELETE FROM {0} WHERE {1} IN ({2})";

		public static string BuildUpsertQuery(string tableName, string[] columns, string values, string[] columnsToUpsert)
		{

			var upsertStatement = new StringBuilder();

			foreach (string propertyName in columnsToUpsert)
			{
				upsertStatement.Append($"{propertyName}=VALUES({propertyName}),");
			}
			MySqlFormatter.RemoveLastCharacter(upsertStatement);
			return string.Format(upsertQuery, tableName, string.Join(",", columns), values, upsertStatement);
		}

		public static string BuildDeleteQuery(string tableName, string column, string values)
		{
			return string.Format(deleteQuery, tableName, column, values);
		}
	}
}

