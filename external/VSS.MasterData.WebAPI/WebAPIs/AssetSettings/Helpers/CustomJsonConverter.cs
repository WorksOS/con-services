using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSS.MasterData.Asset.WebAPI.Helpers
{
	/// <summary>
	/// Custom actions for the API on a JSON object
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class CustomJsonConverter : JsonConverter
	{
		#region Declarations

		private readonly List<Type> _modelSystemTypes = new List<Type> { typeof(string), typeof(object) };

		#endregion Declarations

		#region Constructors


		#endregion Constructors

		#region  Public Methods

		/// <param name="objectType"></param>
		/// <returns></returns>
		public override bool CanConvert(Type objectType)
		{
			return true;
		}

		/// <summary>
		/// Read a Json object
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="objectType"></param>
		/// <param name="existingValue"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jsonObject;
			try
			{
				jsonObject = JObject.Load(reader);
			}
			catch (Exception)
			{
				throw new ArgumentException("Invalid JSON");
			}
			var objDic = jsonObject.Properties().ToDictionary(prop => prop.Name, prop => prop.Value, StringComparer.OrdinalIgnoreCase);
			if ((objDic.Select(dict => dict.Key).Except(objectType.GetProperties().Select(prop => prop.Name), StringComparer.OrdinalIgnoreCase)).Any())
			{
				throw new ArgumentException("Invalid JSON");
			}
			var result = Activator.CreateInstance(objectType);
			StringBuilder errorMsgs = new StringBuilder();
			foreach (var property in objectType.GetProperties())
			{
				SetValue(property, objDic, errorMsgs, result);
			}
			if (errorMsgs.Length > 1)
			{
				throw new ArgumentException(errorMsgs.ToString().TrimEnd(' ', ','));
			}
			return result;
		}

		/// <summary>
		/// Write Json
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JToken t = JToken.FromObject(value);
			t.WriteTo(writer);
		}

		#endregion  Public Methods

		#region  Private Methods

		private void SetValue(PropertyInfo property, Dictionary<string, JToken> objDic, StringBuilder errorMsgs, object result)
		{
			var isRequired = property.CustomAttributes.Any(attr => attr.AttributeType == typeof(RequiredAttribute));
			if (objDic.ContainsKey(property.Name))
			{
				try
				{
					// objValue will never be null
					var objValue = objDic[property.Name];
					if (isRequired && property.PropertyType.IsClass && string.IsNullOrEmpty(objValue.ToString()))
					{
						errorMsgs.Append($"{property.Name} : This is a Required Field, ");
					}
					else if (isRequired && (typeof(Guid?).IsAssignableFrom(property.PropertyType)) && objValue.ToString() == Guid.Empty.ToString())
					{
						errorMsgs.Append($"{property.Name} : Invalid Guid, ");
					}
					else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
					{
						object val = null;
						if (objValue is JArray)
						{
							var listObj = (IList)Activator.CreateInstance(property.PropertyType);
							var jArray = objValue as JArray;
							foreach (var elem in jArray)
							{
								var jObject = elem as JObject;
								if (jObject != null)
								{
									var innerObj = Activator.CreateInstance(property.PropertyType.GetGenericArguments()[0]);
									var jcollection = jObject.Properties().ToDictionary(prop => prop.Name, prop => prop.Value, StringComparer.OrdinalIgnoreCase);
									foreach (var innerProperty in property.PropertyType.GetGenericArguments()[0].GetProperties())
									{
										SetValue(innerProperty, jcollection, errorMsgs, innerObj);
									}
									listObj.Add(innerObj);
								}
								else
								{
									listObj.Add(elem.ToObject(property.PropertyType.GetGenericArguments()[0]));
								}
							}
							val = listObj;
						}
						else if (objValue.ToString() != string.Empty)
						{
							throw new Exception();
						}
						property.SetValue(result, val);
					}
					else if (objValue.ToString() != string.Empty && property.PropertyType.IsClass)
					{

						if (_modelSystemTypes.Contains(property.PropertyType))
						{
							var val = objValue.ToObject(property.PropertyType);
							property.SetValue(result, val);
						}
						else
						{
							var jObject = objValue as JObject;
							if (jObject != null)
							{
								var jcollection = jObject.Properties().ToDictionary(prop => prop.Name, prop => prop.Value, StringComparer.OrdinalIgnoreCase);
								var innerObj = Activator.CreateInstance(property.PropertyType);
								foreach (var innerProperty in property.PropertyType.GetProperties())
								{
									SetValue(innerProperty, jcollection, errorMsgs, innerObj);
								}
								property.SetValue(result, innerObj);
							}
						}
					}
					else if ((typeof(DateTime?).IsAssignableFrom(property.PropertyType) || (typeof(long?).IsAssignableFrom(property.PropertyType)) || (typeof(int?).IsAssignableFrom(property.PropertyType))) && (string)objValue == string.Empty)
					{
						errorMsgs.Append(
							$"{property.Name} : Unable to convert given value to Type {property.PropertyType}, ");
					}
					else
					{
						try
						{
							property.SetValue(result, (((string)objValue == null && (Nullable.GetUnderlyingType(property.PropertyType) != null)) || ((string)objValue == null && property.PropertyType.IsClass)) ? null : objValue.ToObject(property.PropertyType));
						}
						catch (Exception)
						{
							property.SetValue(result, (((string)objValue == null && (Nullable.GetUnderlyingType(property.PropertyType) != null)) || ((string)objValue == null && property.PropertyType.IsClass)) ? null : TypeDescriptor.GetConverter(property.PropertyType).ConvertFromInvariantString((string)objValue));
						}
					}
				}
				catch
				{
					errorMsgs.Append(
						$"{property.Name} : Unable to convert given value to Type {property.PropertyType}, ");
				}
			}
			else if (isRequired)
			{
				errorMsgs.Append($"{property.Name} : This is a Required Field, ");
			}
			else if (typeof(string).IsAssignableFrom(property.PropertyType))
			{
				property.SetValue(result, MasterDataConstants.INVALID_STRING_VALUE);
			}
			else if (typeof(int?).IsAssignableFrom(property.PropertyType))
			{
				property.SetValue(result, MasterDataConstants.INVALID_INT_VALUE);
			}
			else if (typeof(long?).IsAssignableFrom(property.PropertyType))
			{
				property.SetValue(result, MasterDataConstants.INVALID_LONG_VALUE);
			}
			else if (typeof(Guid?).IsAssignableFrom(property.PropertyType))
			{
				property.SetValue(result, Guid.Empty);
			}
			else if (typeof(DateTime?).IsAssignableFrom(property.PropertyType))
			{
				property.SetValue(result, new DateTime(1111, 11, 11));
			}
			else if (typeof(List<Guid>).IsAssignableFrom(property.PropertyType))
			{
				property.SetValue(result, new List<Guid> { Guid.Empty });
			}
		}

		#endregion Private Methods

	}

}
