using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DeviceSettings.Helpers
{
	public class ShouldSerializeContractResolver : DefaultContractResolver
	{
		#region Protected Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="member"></param>
		/// <param name="memberSerialization"></param>
		/// <returns></returns>
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);
			var memberProp = member as PropertyInfo;
			var memberField = member as FieldInfo;


			if (typeof(string).IsAssignableFrom(property.PropertyType) ||
					typeof(int?).IsAssignableFrom(property.PropertyType) ||
					typeof(DateTime?).IsAssignableFrom(property.PropertyType) ||
					typeof(Guid?).IsAssignableFrom(property.PropertyType) ||
					typeof(List<Guid>).IsAssignableFrom(property.PropertyType) ||
					typeof(long?).IsAssignableFrom(property.PropertyType)
					)
			{
				property.ShouldSerialize = obj =>
				{
					object value = memberProp != null
						? memberProp.GetValue(obj, null)
						: memberField != null
							? memberField.GetValue(obj)
							: null;

					if (value is int? && value != null)
					{
						return ((int?)value).Value != MasterDataConstants.INVALID_INT_VALUE;
					}
					else if (value is string)
					{
						if (((string)value).Trim() == "") return false;
						else
							return value.ToString() != MasterDataConstants.INVALID_STRING_VALUE;
					}
					else if (value is DateTime)
					{
						return ((DateTime)value).ToString("MM-dd-yyyy") != MasterDataConstants.INVALID_DATE_TIME_VALUE;
					}
					else if (value is long)
					{
						return (long)value != MasterDataConstants.INVALID_INT_VALUE;
					}
					return true;
				};
			}

			return property;

		}

		#endregion Protected Methods
	}
}
