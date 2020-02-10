using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace Infrastructure.Common.Helpers
{
	public static class UtilHelpers
	{
		public static string GetEnumDescription(Enum value)
		{
			// Get the Description attribute value for the enum value
			FieldInfo fi = value.GetType().GetField(value.ToString());
			DescriptionAttribute[] attributes =
				(DescriptionAttribute[])fi.GetCustomAttributes(
					typeof(DescriptionAttribute), false);

			if (attributes.Length > 0)
			{
				return attributes[0].Description;
			}
			else
			{
				return value.ToString();
			}
		}

		public static IDictionary<T, string> GetEnumDescriptions<T>()
		{
			IDictionary<T, string> enumDescriptions = new Dictionary<T, string>();
			var enumType = typeof(T);
			// Get the Description attribute value for the enum value
			foreach (var enumValue in Enum.GetValues(enumType))
			{
				FieldInfo fi = enumType.GetField(enumValue.ToString());
				DescriptionAttribute[] attributes =
					(DescriptionAttribute[])fi.GetCustomAttributes(
						typeof(DescriptionAttribute), false);
				string description = enumValue.ToString();
				if (attributes.Length > 0)
				{
					description = attributes[0].Description;
				}
				enumDescriptions.Add((T)enumValue, attributes[0].Description);
			}
			return enumDescriptions;
		}


		public static T Clone<T>(this T source)
		{
			var dcs = new DataContractSerializer(typeof(T));
			using (var ms = new System.IO.MemoryStream())
			{
				dcs.WriteObject(ms, source);
				ms.Seek(0, System.IO.SeekOrigin.Begin);
				return (T)dcs.ReadObject(ms);
			}
		}
	}
}
