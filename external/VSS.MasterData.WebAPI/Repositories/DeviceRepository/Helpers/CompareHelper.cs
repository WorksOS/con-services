using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VSS.MasterData.WebAPI.Repository.Device.Helpers
{
	public class CompareHelper
	{
		#region  Public Methods

		public static bool AreObjectsEqual(object objectA, object objectB, params string[] ignoreList)
		{
			var objectTypeA = objectA.GetType();
			var objectTypeB = objectB.GetType();

			List<string> valueA = new List<string>();
			List<string> valueB = new List<string>();

			foreach (PropertyInfo propertyInfo in objectTypeA.GetProperties().Where(
				p => p.CanRead && !ignoreList.Contains(p.Name, StringComparer.OrdinalIgnoreCase)).OrderBy(x=>x.Name))
				valueA.Add(propertyInfo.GetValue(objectA)?.ToString());

			foreach (PropertyInfo propertyInfo in objectTypeB.GetProperties().Where(
				p => p.CanRead && !ignoreList.Contains(p.Name, StringComparer.OrdinalIgnoreCase)).OrderBy(x => x.Name))
				valueB.Add(propertyInfo.GetValue(objectB)?.ToString());

			for (var i = 0; i < valueA.Count(); i++)
			{
				if (valueA[i] != valueB[i])
				{
					return false;
				}
				else
					continue;
			}
			return true;
		}

		public static DateTime ConvertDateTimeForComparison(DateTime dateTimeVal)
		{
			return Convert.ToDateTime(dateTimeVal.ToString("yyyy-MM-dd hh:mm:ss"));
		}

		#endregion  Public Methods
	}
}
