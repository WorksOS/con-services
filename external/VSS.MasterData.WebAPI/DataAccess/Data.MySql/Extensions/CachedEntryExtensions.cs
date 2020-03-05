using Data.MySql.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VSS.MasterData.WebAPI.Data.MySql.Cache;

namespace VSS.MasterData.WebAPI.Data.MySql.Extensions
{
	public static class CachedEntryExtensions
	{
		public static Dictionary<string, string> GetTableNameAndColumns<T>(this IDbTable dbTable)
		{
			Type tType = typeof(T);
			Dictionary<string, string> columnNames;
			if (!CacheEntry.TypeCache.TryGetValue(tType, out columnNames))
			{
				if (columnNames == null)
				{
					columnNames = new Dictionary<string, string>();
				}
				PropertyInfo[] properties = tType.GetProperties();
				var dbTableNameAttr = tType.GetCustomAttribute<DBTableNameAttribute>();
				foreach (PropertyInfo property in properties)
				{
					object[] attributes = property.GetCustomAttributes(true);
					if (attributes.Length <= 0)
					{
						columnNames.Add(property.Name, property.Name);
					}
					foreach (object columnAttr in attributes)
					{
						DBColumnNameAttribute columnNameAttr = columnAttr as DBColumnNameAttribute;
						DBColumnIgnoreAttribute ignoreColumnAttr = columnAttr as DBColumnIgnoreAttribute;
						if (columnNameAttr != null)
						{
							if (ignoreColumnAttr == null || ignoreColumnAttr.Ignore == false)
							{
								columnNames.Add(property.Name, columnNameAttr != null ? columnNameAttr.Name : property.Name);
							}
						}
					}
				}
				if (columnNames != null && columnNames.Count >= 0)
				{
					CacheEntry.TypeCache.TryAdd(tType, columnNames);
				}
			}
			return columnNames;
		}
	}
}
