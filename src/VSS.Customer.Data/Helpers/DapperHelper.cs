using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.Customer.Data.Helpers
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
			if (!EqualityComparer<T>.Default.Equals(value, default(T))) //null and default values excluded
			{
				string val;
				if (typeof(T) == typeof(DateTime))
					val = string.Format("'{0}'", ((DateTime)(object)value).ToString("yyyy-MM-dd HH:mm:ss"));
				//no need quote for numeric or bool
				else if (TypeHelper.IsNumeric(typeof(T)) || (Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T)) == typeof(bool))
					val = value.ToString();
				else if (addQuoteToValue)
					val = string.Format("'{0}'", value);
				else
					val = value.ToString();
				sb.Append(string.Format("{0}{1}={2}", commaNeeded ? "," : "", key, val));
				added = true;
			}
			return added;
		}
	}
}
