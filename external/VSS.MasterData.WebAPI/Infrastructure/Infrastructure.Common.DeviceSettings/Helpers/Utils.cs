using Infrastructure.Common.DeviceSettings.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;


namespace Infrastructure.Common.DeviceSettings.Helpers
{
    public static class Utils
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

        public static string GetEnumValuesAsKeyValueString(Type enumType)
        {
            StringBuilder result = new StringBuilder(string.Empty);
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                result.Append((int)enumValue + "-" + enumValue.ToString() + ", ");
            }
            if (result.Length > 1)
            {
                return result.ToString().Substring(0, result.Length - 2);
            }
            return result.ToString();
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

        public static double GetDigitsCount(int value)
        {
            return Math.Floor(Math.Log10(value) + 1);
        }

        public static int GetPrecisionCount(decimal value)
        {
            return BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
        }
    }
}
