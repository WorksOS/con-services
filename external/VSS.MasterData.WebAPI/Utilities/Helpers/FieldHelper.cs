using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using VSS.MasterData.WebAPI.Data.MySql;
using VSS.MasterData.WebAPI.Utilities.Attributes;

namespace VSS.MasterData.WebAPI.Utilities.Helpers
{
	public static class FieldHelper
	{
		public static bool IsValidValuesFilled(object client, object db, ILogger logger)
		{
			try
			{
				if (!typeof(IDbTable).IsAssignableFrom(db.GetType()))
					return false;
				var properties = client.GetType().GetProperties()
					.Select(pi => new
					{
						Property = pi,
						Attribute = pi.GetCustomAttributes(typeof(DbFieldNameAttribute), true)
						.FirstOrDefault() as DbFieldNameAttribute
					})
					.Where(x => x.Attribute != null).ToList();
				foreach (var clientProperty in properties)
				{
					var dbProperty = db.GetType().GetProperty(clientProperty.Attribute.fieldName);
					var cProperty = clientProperty.GetType().GetProperty(clientProperty.Attribute.fieldName);
					if (clientProperty.Attribute.expectedType == null)
					{
						var clientObjectValue = clientProperty.Property.GetValue(client);
						if (clientObjectValue != null)
						{
							dbProperty.SetValue(db, clientObjectValue);
						}
						else
						{
							clientProperty.Property.SetValue(client, dbProperty.GetValue(db));
						}
					}
					else if (clientProperty.Property.PropertyType == typeof(string)
						&& clientProperty.Attribute.expectedType.IsEnum)
					{
						string clientObjectValue = (string)clientProperty.Property.GetValue(client);
						if (!string.IsNullOrEmpty(clientObjectValue))
						{
							object resultInputType
								= Activator.CreateInstance(clientProperty.Attribute.expectedType);
							resultInputType = Enum.Parse(clientProperty.Attribute.expectedType,
								clientObjectValue.ToString(), true);
							dbProperty.SetValue(db, (int)resultInputType);
						}
						else
						{
							clientProperty.Property.SetValue(client,
								(Enum.ToObject(clientProperty.Attribute.expectedType, dbProperty.GetValue(db)))
								.ToString());
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				throw new Exception(ex.Message);
			}
			return true;
		}

		public static void ReplaceEmptyFieldsByNull(object inputObject, params string[] ignoreList)
		{
			foreach (PropertyInfo propertyInfo in inputObject.GetType().GetProperties().Where(p => p.CanRead))
			{
				if (!ignoreList.Contains(propertyInfo.Name))
				{
					if (propertyInfo.GetValue(inputObject)?.ToString() == string.Empty)
					{
						propertyInfo.SetValue(inputObject, null);
					}
				}
			}
		}
	}
}
