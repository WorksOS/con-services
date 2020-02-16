using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Web.Script.Serialization;


namespace VSS.Hosted.VLCommon.ExtensionMethodStringScrub
{
  public static class StringScrubExtension
  {
    public static string ToString(this String str, bool ScrubStringData)
    {
      StringBuilder sb = new StringBuilder();
      try
      {
        if (str != null)
        {
          char[] values = str.ToCharArray();

          foreach (var value in values)
          {
            if (value >= 32 && value < 127)
            {
              char[] littleArray = new char[1];
              littleArray[0] = value;

              sb.Append(new string(littleArray));
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      return System.Security.SecurityElement.Escape(sb.ToString());
    }
  }
}

namespace VSS.Hosted.VLCommon
{

  public static class DotNetExtensions
  {
    private static readonly Regex guidPattern = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
    public const int NullKeyDate = 99991231;
    #region Extension Methods
    public static DateTime SubtractHours(this DateTime value, int hours)
    {
      return value.AddHours(hours * -1);
    }

    public static bool IsNotNullOrMinValue(this DateTime? value)
    {
      if (value.HasValue == false) return false;
      if (value.HasValue && value.Value.IsMinValue()) return false;
      return true;
    }

    public static int KeyDate(this DateTime value)
    {
      return (value.Year * 10000) + (value.Month * 100) + (value.Day);
    }

    public static int KeyDate(this DateTime? value)
    {
      if (value.HasValue)
        return value.Value.KeyDate();

      return NullKeyDate;
    }

    public static DateTime FromKeyDate(this int keyDate)
    {
      //return DateTime.ParseExact(keyDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
      return new DateTime(keyDate.KeyDateYear(), keyDate.KeyDateMonth(), keyDate.KeyDateDay());
    }

    // keyDate = yy * 10000 + MM * 100 + dd
    // yy = keyDate / 10000
    public static int KeyDateYear(this int keyDate)
    {
      if (keyDate < 19000000 || keyDate > 99991231)
        throw new ArgumentOutOfRangeException(string.Format("Invalid keyDate {0}", keyDate));

      return keyDate / 10000;
    }

    // keyDate = yy * 10000 + MM * 100 + dd
    // MM = (keyDate - (yy * 10000)) / 100
    public static int KeyDateMonth(this int keyDate)
    {
      if (keyDate < 19000000 || keyDate > 99991231)
        throw new ArgumentOutOfRangeException(string.Format("Invalid keyDate {0}", keyDate));

      return (keyDate - (keyDate.KeyDateYear() * 10000)) / 100;
    }

    // keyDate = yy * 10000 + MM * 100 + dd
    // dd = (keyDate - (yy * 10000) - (MM * 100))
    public static int KeyDateDay(this int keyDate)
    {
      if (keyDate < 19000000 || keyDate > 99991231)
        throw new ArgumentOutOfRangeException(string.Format("Invalid keyDate {0}", keyDate));

      return (keyDate - (keyDate.KeyDateYear() * 10000) - (keyDate.KeyDateMonth() * 100));
    }

    public static string KeydateToIso8601Date(this int keyDate)
    {
      if (keyDate < 19000000 || keyDate > 99991231)
        throw new ArgumentOutOfRangeException(string.Format("Invalid keyDate {0}", keyDate));

      return string.Format("{0:0000}-{1:00}-{2:00}", keyDate.KeyDateYear(), keyDate.KeyDateMonth(), keyDate.KeyDateDay());
    }

    public static bool IsNotMinValue(this DateTime value)
    {
      return value.IsMinValue() == false;
    }

    public static bool IsMinValue(this DateTime value)
    {
      return value == DateTime.MinValue;
    }

    public static DateTime? EndOfDay(this DateTime? value)
    {
      if (value.HasValue)
        return value.Value.EndOfDay();

      return null;
    }

    public static DateTime? StartOfDay(this DateTime? value)
    {
      if (value.HasValue)
        return value.Value.StartOfDay();

      return null;
    }

    public static DateTime EndOfPreviousDay(this DateTime value)
    {
      if (value.Equals(DateTime.MinValue))
        return value;

      return EndOfDay(value.AddDays(-1));
    }

    public static DateTime EndOfDay(this DateTime value)
    {
      if (value.Equals(DateTime.MaxValue))
        return DateTime.MaxValue;

      return value.Date.AddDays(1).AddSeconds(-1);
    }

    public static DateTime StartOfDay(this DateTime value)
    {
      if (value.Equals(DateTime.MinValue))
        return DateTime.MinValue;

      return value.Date;
    }

    public static bool IsTimeOfDayInRange(this DateTime value, TimeSpan start, TimeSpan end)
    {
      return value.TimeOfDay >= start && value.TimeOfDay <= end;
    }

    public static double? Truncate(this double? number, int digits)
    {
      if (number.HasValue == false)
        return null;

      double stepper = Math.Pow(10.0, (double)digits);
      double temp = Math.Floor(stepper * number.Value);
      double result = temp / stepper;

      return result;
    }

    public static string RemoveAllWhitespace(this string value)
    {
      return value.Replace(" ", string.Empty).Replace("\t", string.Empty).Replace(Environment.NewLine, string.Empty);
    }

    public static string[] Split(this string data, int blockSize)
    {
      if (data.Length < blockSize)
        return new string[] { data };

      string[] splitData = new string[(int)Math.Round(data.Length / (double)blockSize) + 1];

      int block = 0;
      for (int i = 0; i < splitData.Length; ++i)
      {
        splitData[i] = data.Substring(block, blockSize);
        block += blockSize;
      }

      return splitData;
    }

    public static bool ContainsIgnoreCase(this string container, string contained)
    {
      return container.IndexOf(contained, StringComparison.CurrentCultureIgnoreCase) >= 0;
    }

    public static string ToCsvString(this IEnumerable obj)
    {
      if (obj == null)
        return string.Empty;
      List<string> items = new List<string>();
      foreach (object data in obj)
      {
        items.Add(data.ToString());
      }
      return String.Join(",", items.ToArray());
    }

    public static string ToCsvSerializedString(this IEnumerable obj)
    {
      if (obj == null)
        return string.Empty;
      List<string> items = new List<string>();
      foreach (object data in obj)
      {
        items.Add(JavaScriptObjectSerializer.Serialize(data));
      }
      return String.Join(",", items.ToArray());
    }

    public static string ToCsvSerializedString(this DataRowCollection obj)
    {
      if (obj == null)
        return string.Empty;
      List<string> items = new List<string>();
      foreach (DataRow data in obj)
      {
        items.Add(JavaScriptObjectSerializer.Serialize(data.ItemArray));
      }
      return String.Join(",", items.ToArray());
    }

    public static bool IsEqualTo(this DateTime d1, DateTime d2)
    {
      return (string.Compare(d1.ToLongDateString(), d2.ToLongDateString()) == 0)
        && (string.Compare(d1.ToLongTimeString(), d2.ToLongTimeString()) == 0);
    }

    public static bool IsExactlyEqualTo(this DateTime d1, DateTime d2)
    {
      return (d1.Hour == d2.Hour) && (d1.Minute == d2.Minute) && (d1.Second == d2.Second)
        && (d1.Month == d2.Month) && (d1.Day == d2.Day) && (d1.Year == d2.Year);
    }

    public static bool IsLaterThan(this DateTime d1, DateTime d2)
    {
      return (string.Compare(d1.ToLongDateString(), d2.ToLongDateString()) > 0)
        || (string.Compare(d1.ToLongTimeString(), d2.ToLongTimeString()) > 0);
    }

    public static bool IsEarlierThan(this DateTime d1, DateTime d2)
    {
      return (string.Compare(d1.ToLongDateString(), d2.ToLongDateString()) < 0)
        || (string.Compare(d1.ToLongTimeString(), d2.ToLongTimeString()) < 0);
    }

    public static T DeepCopy<T>(this T original) where T : class
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(memoryStream, original);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return (T)binaryFormatter.Deserialize(memoryStream);
      }
    }

    public static XElement GetElement(this XElement parent, XName element)
    {
      IEnumerable<XElement> nodes = parent.Descendants(element);
      if (nodes.Count() > 0)
        return nodes.First();

      return new XElement(element);
    }

    public static string GetStringElement(this XElement parent, XName element)
    {
      IEnumerable<XElement> desc = parent.Descendants(element);
      if (desc.Count() > 0)
        return desc.First().Value;

      return string.Empty;
    }

    public static double? GetDoubleElement(this XElement parent, XName element)
    {
      double result = double.NaN;

      IEnumerable<XElement> desc = parent.Descendants(element);

      return (desc.Count() > 0 && double.TryParse(desc.First().Value, out result)) ? result : (double?)null;
    }

    public static DateTime? GetDateTimeElement(this XElement parent, XName element)
    {
      DateTime result = DateTime.MinValue;

      IEnumerable<XElement> desc = parent.Descendants(element);

      return (desc.Count() > 0 && DateTime.TryParse(desc.First().Value, out result)) ? result : (DateTime?)null;
    }

    public static DateTime? GetUTCDateTimeElement(this XElement parent, XName element)
    {
      DateTime? result = GetDateTimeElement(parent, element);
      return result.HasValue ? result.Value.ToUniversalTime() : result;
    }

    public static DateTime? GetDateTimeElement(this XElement parent, XName element, IFormatProvider culture)
    {
      DateTime result = DateTime.MinValue;

      IEnumerable<XElement> desc = parent.Descendants(element);

      return (desc.Count() > 0 && DateTime.TryParse(desc.First().Value, culture, DateTimeStyles.None, out result)) ? result : (DateTime?)null;
    }

    public static DateTime? GetUTCDateTimeAttributeExact(this XElement parent, XName attributeName)
    {
      DateTime result = DateTime.MinValue;

      IEnumerable<XAttribute> desc = parent.Attributes(attributeName);

      if (desc.Any() == false) return (DateTime?)null;

      string timestamp = desc.First().Value;
      if (string.IsNullOrEmpty(timestamp) == false)
      {
        timestamp = timestamp.Substring(0, 19) + "Z";

        result = DateTime.ParseExact(timestamp,
          "yyyy-MM-dd'T'HH:mm:ss'Z'",
          CultureInfo.InvariantCulture,
          DateTimeStyles.AdjustToUniversal);

        return result != DateTime.MinValue ? result : (DateTime?)null;
      }
      else return (DateTime?)null;
    }

    public static TimeSpan? GetTimeSpanElement(this XElement parent, XName element)
    {
      TimeSpan result = TimeSpan.MinValue;

      IEnumerable<XElement> desc = parent.Descendants(element);

      return (desc.Count() > 0 && TimeSpan.TryParse(desc.First().Value, out result)) ? result : (TimeSpan?)null;
    }

    public static long? GetLongElement(this XElement parent, XName element)
    {
      long result = long.MinValue;

      IEnumerable<XElement> desc = parent.Descendants(element);

      return (desc.Count() > 0 && long.TryParse(desc.First().Value, out result)) ? result : (long?)null;
    }

    public static int? GetIntElement(this XElement parent, XName element)
    {
      int result = int.MinValue;

      IEnumerable<XElement> desc = parent.Descendants(element);

      return (desc.Count() > 0 && int.TryParse(desc.First().Value, out result)) ? result : (int?)null;
    }

    public static byte? GetByteElement(this XElement parent, XName element)
    {
      byte result = 0;
      IEnumerable<XElement> desc = parent.Descendants(element);

      return (desc.Count() > 0 && byte.TryParse(desc.First().Value, out result)) ? result : (byte?)null;
    }

    public static bool? GetBooleanElement(this XElement parent, XName element)
    {
      bool result = false;

      IEnumerable<XElement> desc = parent.Descendants(element);

      return (desc.Count() > 0 && bool.TryParse(desc.First().Value, out result)) ? result : (bool?)null;
    }

    public static bool? GetBooleanAttribute(this XElement parent, XName attributeName)
    {
      bool result = false;
      IEnumerable<XAttribute> attribute = parent.Attributes(attributeName);

      return (attribute.Count() > 0 && bool.TryParse(attribute.First().Value, out result)) ? result : (bool?)null;
    }

    public static byte? GetByteAttribute(this XElement parent, XName attributeName)
    {
      byte result = 0;
      IEnumerable<XAttribute> attribute = parent.Attributes(attributeName);

      return (attribute.Count() > 0 && byte.TryParse(attribute.First().Value, out result)) ? result : (byte?)null;
    }

    public static short? GetShortAttribute(this XElement parent, XName attributeName)
    {
      short result = 0;
      IEnumerable<XAttribute> attribute = parent.Attributes(attributeName);

      return (attribute.Count() > 0 && short.TryParse(attribute.First().Value, out result)) ? result : (short?)null;
    }

    public static int? GetIntAttribute(this XElement parent, XName attributeName)
    {
      int result = 0;
      IEnumerable<XAttribute> attribute = parent.Attributes(attributeName);

      return (attribute.Count() > 0 && int.TryParse(attribute.First().Value, out result)) ? result : (int?)null;
    }

    public static long? GetLongAttribute(this XElement parent, XName attributeName)
    {
      long result = 0;
      IEnumerable<XAttribute> attribute = parent.Attributes(attributeName);

      return (attribute.Count() > 0 && long.TryParse(attribute.First().Value, out result)) ? result : (long?)null;
    }

    public static double? GetDoubleAttribute(this XElement parent, XName attributeName)
    {
      double result = 0;
      IEnumerable<XAttribute> attribute = parent.Attributes(attributeName);

      return (attribute.Count() > 0 && double.TryParse(attribute.First().Value, out result)) ? result : (double?)null;
    }

    public static string GetStringAttribute(this XElement parent, XName attributeName)
    {
      IEnumerable<XAttribute> desc = parent.Attributes(attributeName);
      if (desc.Count() > 0)
        return desc.First().Value;

      return string.Empty;
    }

    public static DateTime? GetDateTimeAttribute(this XElement parent, XName attributeName, IFormatProvider culture)
    {
      DateTime result = DateTime.MinValue;

      IEnumerable<XAttribute> desc = parent.Attributes(attributeName);

      return (desc.Count() > 0 && DateTime.TryParse(desc.First().Value, culture, DateTimeStyles.None, out result)) ? result : (DateTime?)null;
    }

    public static DateTime? GetDateTimeAttribute(this XElement parent, XName attributeName)
    {
      DateTime result = DateTime.MinValue;

      IEnumerable<XAttribute> desc = parent.Attributes(attributeName);

      return (desc.Count() > 0 && DateTime.TryParse(desc.First().Value, out result)) ? result : (DateTime?)null;
    }

    public static DateTime? GetUTCDateTimeAttribute(this XElement parent, XName attributeName)
    {
      DateTime? result = GetDateTimeAttribute(parent, attributeName);
      return result.HasValue ? result.Value.ToUniversalTime() : result;
    }

    public static TimeSpan? GetTimeSpanAttribute(this XElement parent, XName attributeName)
    {
      TimeSpan result = TimeSpan.MinValue;

      IEnumerable<XAttribute> desc = parent.Attributes(attributeName);

      return (desc.Count() > 0 && TimeSpan.TryParse(desc.First().Value, out result)) ? result : (TimeSpan?)null;
    }

    public static string ToIso8601DateTimeString(this DateTime dateTimeUTC)
    {
      // CAUTION - this assumes the DateTime passed in is already UTC!!
      return string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", dateTimeUTC);
    }

    public static string ToIso8601DateTimeString(this DateTime? dateTimeUTC)
    {
      // nullable overload version
      // CAUTION - this assumes the DateTime passed in is already UTC!!
      return string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", dateTimeUTC);
    }

    public static T ToEnum<T>(this string enumTokenName) where T : struct
    {
      T enumToken = default(T);
      if (!Enum.TryParse<T>(enumTokenName, true, out enumToken) ||
          !Enum.IsDefined(typeof(T), enumToken))
      {
        throw new InvalidCastException(string.Format("Can not parse {0} with {1}.", enumTokenName, enumToken.GetType().FullName));
      }

      return enumToken;
    }

    public static T ToEnum<T>(this int enumTokenName) where T : struct
    {
      T enumToken = default(T);
      try
      {
        enumToken = (T)Enum.ToObject(typeof(T), enumTokenName);
      }
      catch (Exception)
      {
        throw new InvalidCastException(string.Format("Can not parse {0} with {1}.", enumTokenName, enumToken.GetType().FullName));
      }
      return enumToken;
    }

    public static T ToDeviceTypeEnum<T>(this string enumTokenName) where T : struct
    {
      T enumToken = default(T);
      enumTokenName = enumTokenName.Replace("+", "PLUS").Replace("-", "HYPHEN").Replace("3P", "THREEP");
      if (!Enum.TryParse<T>(enumTokenName, true, out enumToken) ||
          !Enum.IsDefined(typeof(T), enumToken))
      {
        throw new InvalidCastException(string.Format("Can not parse {0} with {1}.", enumTokenName, enumToken.GetType().FullName));
      }

      return enumToken;
    }

    public static String ToDeviceTypeString<T>(this object obj) where T : struct
    {
      if (obj == null)
        return string.Empty;

      string enumTokenName = obj.ToString();

      if (string.IsNullOrEmpty(enumTokenName.ToString()))
        return string.Empty;

      try
      {

        return enumTokenName.ToEnum<T>().ToString().Replace("PLUS", "+").Replace("HYPHEN", "-").Replace("THREEP", "3P");
      }
      catch
      {
        throw new InvalidOperationException("Invalid deviceType");
      }
    }

    public static bool IsGuid(this string inputString)
    {
      return guidPattern.IsMatch(inputString);
    }

    public static string TruncateToLength(this string inputString, int lengthToTruncate)
    {
      if (inputString == null)
      {
        return null;
      }

      if (lengthToTruncate < 0)
      {
        return inputString;
      }

      if (inputString.Length <= lengthToTruncate)
      {
        return inputString;
      }

      return inputString.Substring(0, lengthToTruncate);
    }

    private static Regex numericRegex = new Regex(@"^[-]?\d+$", RegexOptions.Compiled | RegexOptions.Singleline);
    private static Regex stringNoSpacesRegex = new Regex(@"^[a-zA-Z_]+$", RegexOptions.Compiled | RegexOptions.Singleline);
    private static Regex stringWithSpacesRegex = new Regex(@"^[a-zA-Z ]+$", RegexOptions.Compiled | RegexOptions.Singleline);

    public static bool isNumeric(this string inputString)
    {
      if (string.IsNullOrWhiteSpace(inputString))
        return false;

      return numericRegex.Match(input: inputString).Success;
    }

    public static bool isStringWithSpaces(this string inputString)
    {
      if (string.IsNullOrWhiteSpace(inputString))
        return false;

      return stringWithSpacesRegex.Match(input: inputString).Success;
    }

    public static bool isStringWithNoSpaces(this string inputString)
    {
      if (string.IsNullOrWhiteSpace(inputString))
        return false;

      return stringNoSpacesRegex.Match(input: inputString).Success;
    }

    public static bool isDateTimeValid(this string inputString)
    {
      if (string.IsNullOrWhiteSpace(inputString))
        return false;

      var success = true;
      try
      {
        var dt = DateTime.Parse(inputString).ToUniversalTime();
        if (dt.Kind != DateTimeKind.Utc)
          success = false;
      }
      catch { success = false; }
      return success;
    }

    public static bool isDateTimeISO8601(this string inputStringUTC, string format, out DateTime resultDateTimeUTC)
    {
      if (string.IsNullOrWhiteSpace(inputStringUTC))
      {
        resultDateTimeUTC = DateTime.MinValue;
        return false;
      }

      return DateTime.TryParseExact(inputStringUTC, format, new CultureInfo("en-US"), DateTimeStyles.AdjustToUniversal, out resultDateTimeUTC);
    }

    public static bool isDateTimeISO8601(this string inputStringUTC, string format, IFormatProvider culture, out DateTime resultDateTimeUTC)
    {
      if (string.IsNullOrWhiteSpace(inputStringUTC))
      {
        resultDateTimeUTC = DateTime.MinValue;
        return false;
      }

      return DateTime.TryParseExact(inputStringUTC, format, culture, DateTimeStyles.AdjustToUniversal, out resultDateTimeUTC);
    }

    public static bool isDateTimeISO8601(this string inputStringUTC, string format, IFormatProvider culture, DateTimeStyles dateTimeStyle, out DateTime resultDateTimeUTC)
    {
      if (string.IsNullOrWhiteSpace(inputStringUTC))
      {
        resultDateTimeUTC = DateTime.MinValue;
        return false;
      }

      return DateTime.TryParseExact(inputStringUTC, format, culture, dateTimeStyle, out resultDateTimeUTC);
    }

    public static int GetWeekOfYear(this DateTime date)
    {
      GregorianCalendar cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
      return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    public static DateTime GetMondayOfWeek(this DateTime originalDate)
    {
      // this computes the start of the week in which the startDate falls.
      // start of a week in VisionLink is Monday, so add one to the result,
      // and then, if the startDate is on a Sunday, we need to adjust it
      // to start on Monday of last week instead of Monday tomorrow.
      DateTime startWeek = originalDate.AddDays((-((int)originalDate.DayOfWeek) + 1));
      if (originalDate.DayOfWeek == DayOfWeek.Sunday)
        startWeek = startWeek.AddDays(-7);

      return startWeek;
    }

    public static DateTime GetFirstDayOfMonth(this DateTime date)
    {
      return date.AddDays(-date.Day + 1);
    }

    /// <summary>
    /// If <paramref name="type"/> is <c>Nullable&lt;T&gt;</c>, returns <c>T</c>; otherwise returns <paramref name="type"/>.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Type GetTypeOrNullableType(this Type type)
    {
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        return type.GenericTypeArguments[0];
      else
        return type;
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="type"/> implements the generic interface <paramref name="genericType"/>.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="genericType"></param>
    /// <returns></returns>
    public static bool ImplementsGenericInterface(this Type type, Type genericType)
    {
      return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericType);
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="type"/> has a public parameter-less constructor.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool HasDefaultConstructor(this Type type)
    {
      return type.GetConstructor(Type.EmptyTypes) != null;
    }

    /// <summary>
    /// Invokes <paramref name="type"/>'s public parameter-less constructor or throws
    /// <c>InvalidOperationException</c> if <paramref name="type"/> does not have one.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object InvokeDefaultConstructor(this Type type)
    {
      if (type.HasDefaultConstructor() == false) throw new InvalidOperationException("Type " + type + " doesn't have a default constructor");
      return type.GetConstructor(Type.EmptyTypes).Invoke(null);
    }

    #endregion

    #region Some other utils
    public static string GetFirstNotNullFromTheParameters(params string[] parameters)
    {
      return parameters.FirstOrDefault(f => !string.IsNullOrEmpty(f)) ?? string.Empty;
    }

    #endregion
  }
}
