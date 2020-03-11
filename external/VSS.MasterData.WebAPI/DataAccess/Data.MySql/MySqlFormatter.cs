using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.Data.MySql
{
	public class MySqlFormatter
	{
		private static readonly Dictionary<string, Func<object, string>> ObjtoMySqlFormatter = new Dictionary<string, Func<object, string>>
		{
			["DateTime"] = (dateTime) => $"'{Convert.ToDateTime(dateTime):yyyy-MM-dd HH:mm:ss.ffffff}'",
			["Guid"] = (guid) => $"0x{((Guid)guid):N}",
			["String"] = (str) => !string.IsNullOrEmpty(Convert.ToString(str)) ? $"'{MySqlHelper.EscapeString(str.ToString())}'" : "NULL",
			["Default"] = (allOtherTypes) => allOtherTypes != null ? $"{allOtherTypes}" : "NULL"
		};

		public static string ConvertValueToMySqlFormatstring(object value)
		{
			var formatterFound = ObjtoMySqlFormatter.TryGetValue(value == null ? "NULL" : value.GetType().Name,
				out Func<object, string> funcToUse);

			return formatterFound ? funcToUse(value) : ObjtoMySqlFormatter["Default"](value);
		}

		public static void RemoveLastCharacter(StringBuilder rowOfValues)
		{
			if (rowOfValues.Length > 0)
			{
				//remove last comma
				rowOfValues.Remove(rowOfValues.Length - 1, 1);
			}
		}

	}
}
