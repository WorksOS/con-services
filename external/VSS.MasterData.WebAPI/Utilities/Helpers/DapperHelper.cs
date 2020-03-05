using System;
using System.Text;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace VSS.MasterData.WebAPI.Utilities.Helpers
{
	public static class SqlBuilder
	{
		/// <summary>
		/// Helper to create update statements in SQL. Adds 'key'='value' tokens
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="key"></param>
		/// <param name="sb">the stringbuilder where the value gets appended to</param>
		/// <param name="commaNeeded">do i prefix a comma before adding my key-value</param>
		/// <param name="addQuoteToValue">override false for string values such as functions that shouldn't be quoted</param>
		/// <returns>true if we added something, false if not</returns>
		public static bool AppendValueParameter<T>(T value, string key, StringBuilder sb, bool commaNeeded, bool addQuoteToValue = true)
		{
			bool added = false;
			if (value == null)
			{
				string val = "null";// val2 = Convert.DBNull.ToString();
				sb.Append($"{(commaNeeded ? "," : "")}{key}={val}");
				added = true;
			}
			else if (value.ToString().Trim() == "")
			{
				string val = "null";// val2 = Convert.DBNull.ToString();
				sb.Append($"{(commaNeeded ? "," : "")}{key}={val}");
				added = true;
			}
			else if (!String.Equals(value.ToString(), "11-11-1111") && !String.Equals(value.ToString(), "-9999999") && !String.Equals(value.ToString(), "$#$#$")) //null and default values excluded
			{
				string val;
				if (typeof(T) == typeof(DateTime))
					val = $"'{((DateTime)(object)value).ToString("yyyy-MM-dd HH:mm:ss")}'";
				//no need quote for numeric or bool
				else if (TypeHelper.IsNumeric(typeof(T)) || (Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T)) == typeof(bool))
					val = value.ToString();
				else if ((Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T)) == typeof(Guid))
					val = ((Guid)(object)value).ToStringAndWrapWithUnhex();
				else if (value.ToString().Contains(@"\") || addQuoteToValue)
				{
					var valueReplacedWithSlash = value.ToString().Replace(@"\", @"\\");
					var valueReplacedWithQuotes = valueReplacedWithSlash.Replace("'", "''");
					val = $"'{valueReplacedWithQuotes}'";
				}
				else
					val = value.ToString();
				if (value.ToString() != "11-11-1111" && value.ToString() != "-9999999" && value.ToString() != "$#$#$")
					sb.Append($"{(commaNeeded ? "," : "")}{key}={val}");
				added = true;
			}
			return added;
		}
	}
}

