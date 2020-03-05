using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace VSS.MasterData.WebAPI.Data.Confluent
{
	internal static class ConfigUtility
	{
		public static NameValueCollection ConvertToNameValueCollection(IConfiguration configuration, string sectionName)
		{
			var removeSectionName = $"{sectionName}:";
			var configurationSection = configuration.GetSection(sectionName);
			var nameValueCollection = new NameValueCollection();
			foreach (KeyValuePair<string, string> keyValuePair in configurationSection.AsEnumerable())
			{
				if (keyValuePair.Value != null)
				{
					nameValueCollection.Add(
						!string.IsNullOrEmpty(removeSectionName)
							? keyValuePair.Key.Replace(removeSectionName, string.Empty)
							: keyValuePair.Key,
						keyValuePair.Value);
				}
			}
			return nameValueCollection;
		}
	}
}
