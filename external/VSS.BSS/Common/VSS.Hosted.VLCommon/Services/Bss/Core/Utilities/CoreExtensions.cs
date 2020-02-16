using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using VSS.Hosted.VLCommon.Bss.Schema.V2;


namespace VSS.Hosted.VLCommon.Bss
{
  public static class CoreExtensions
  {
    public static bool IsStringEqual(this string str1, string str2, bool ignoreCase = true)
    {
      return string.Compare(str1, str2, ignoreCase) == 0;
    }

    public static bool IsNotDefined(this PrimaryContact contact)
    {
        return (contact == null || (contact.FirstName.IsNotDefined() &&
                        contact.LastName.IsNotDefined() &&
                        contact.Email.IsNotDefined()));
    }

    public static bool IsDefined(this string str)
    {
      return !string.IsNullOrWhiteSpace(str);
    }

    public static bool IsNotDefined(this string str)
    {
      return string.IsNullOrWhiteSpace(str);
    }

    public static bool IsDefined(this long? id)
    {
      return id != default(long?);
    }

    public static bool IsNotDefined(this long? id)
    {
      return id == default(long?);
    }

    public static bool IsDefined(this long id)
    {
      return id != default(long);
    }

    public static bool IsNotDefined(this long id)
    {
      return id == default(long);
    }

    public static string DataContractObjectToXml<T>(this T dataContractObject)
    {
      var dataContractSerializer = new DataContractSerializer(typeof(T));

      string xmlString;

      using (var memoryStream = new MemoryStream())
      {
        dataContractSerializer.WriteObject(memoryStream, dataContractObject);
        memoryStream.Seek(0, SeekOrigin.Begin);
        xmlString = XElement.Load(memoryStream, LoadOptions.PreserveWhitespace).ToString();
      }

      return xmlString;
    }

    public static string ToFormattedString(this IEnumerable<string> input)
    {
      return string.Join(" ", input);
    }

    public static string ToNewLineString(this IEnumerable<string> input)
    {
      return string.Join(string.Format("{0}", Environment.NewLine), input);
    }

    public static string ToNewLineTabbedString(this IEnumerable<string> input)
    {
      return string.Concat("\t", string.Join(string.Format("\n\t"), input));
    }

    public static bool IsSuccessful(this IList<ActivityResult> activityResults)
    {
      if (activityResults.Count == 0)
        return false;

      var containsErrorOrException = activityResults.Any(x => x.Type == ResultType.Error || x.Type == ResultType.Exception);
      
      return containsErrorOrException == false;
    }

    public static string GetErrorOrExceptionSummary(this IList<ActivityResult> activityResults)
    {
      var errorOrException = activityResults.FirstOrDefault(x => x.Type == ResultType.Error || x.Type == ResultType.Exception);
      return errorOrException != null ? errorOrException.Summary : null;
    }

    public static void MapCustomer(this CustomerDto dto, Customer customer)
    {
      dto.BssId = customer.BSSID;
      dto.Type = (CustomerTypeEnum)customer.fk_CustomerTypeID;
      dto.Name = customer.Name;
      dto.DealerNetwork = (DealerNetworkEnum)customer.fk_DealerNetworkID;
      dto.NetworkDealerCode = customer.NetworkDealerCode;
      dto.NetworkCustomerCode = customer.NetworkCustomerCode;
      dto.DealerAccountCode = customer.DealerAccountCode;
      dto.CustomerUId = customer.CustomerUID;
      dto.PrimaryEmailContact = customer.PrimaryEmailContact;
      dto.FirstName = customer.FirstName;
      dto.LastName = customer.LastName;
    }

    public static object PropertyValueByName(this object instance, string propertyName)
    {
      var property = instance.GetType().GetProperty(propertyName);
      
      if (property == null) return null;
      
      return property.GetValue(instance, null);
    }

    public static string[] PropertiesAndValues(this object instance, string prefix = "")
    {
      var summary = new List<string>();

      foreach (var property in instance.GetType().GetProperties())
      {
        try
        {
          string name = !string.IsNullOrWhiteSpace(prefix) ? string.Format("{0}.{1}", prefix, property.Name) : property.Name;
          var value = property.GetValue(instance, BindingFlags.Public | BindingFlags.GetProperty, null, null, CultureInfo.InvariantCulture);

          if (property.PropertyType.IsClass && 
              property.PropertyType.Namespace != null && 
              property.PropertyType.Namespace.StartsWith("VSS."))
          {
            summary.AddRange(PropertiesAndValues(value, name));
          }   
          else
          {
            summary.Add(string.Format(@"{0}: {1}", name, value));
          }
        }
        catch { summary.Add("Error encountered in Summary method."); }
      }
      return summary.ToArray();
    }

    public static string ToText<TMessage>(this IEnumerable<ActivityResult> results, TMessage sourceMessage)
    {
      var sb = new StringBuilder();

      sb.AppendLine();
      sb.AppendFormat("{0} {1} {0}{2}", "************", typeof(TMessage).Name, Environment.NewLine);
      sb.AppendFormat("Source Message: {0}{1}", sourceMessage.DataContractObjectToXml(), Environment.NewLine);

      foreach (var result in results)
      {
        switch (result.Type)
        {
          case ResultType.Debug:

            sb.AppendFormat("{0} DEBUG {1}{2}", result.DateTimeUtc, result.Summary, Environment.NewLine);
            break;

          case ResultType.Information:

            sb.AppendFormat("{0} INFO {1}{2}", result.DateTimeUtc, result.Summary, Environment.NewLine);
            break;

          case ResultType.Warning:

            sb.AppendFormat("{0} WARNING {1}{2}", result.DateTimeUtc, result.Summary, Environment.NewLine);
            break;

          case ResultType.Error:

            var errorResult = result as BssErrorResult;
            if (errorResult != null)
            {
              sb.AppendFormat("{0} BSS ERROR FailureCode:{1} {2}{3}", errorResult.DateTimeUtc, errorResult.FailureCode, errorResult.Summary, Environment.NewLine);
              break;
            }

            sb.AppendFormat("{0} ERROR {1}{2}", result.DateTimeUtc, result.Summary, Environment.NewLine);
            break;

          case ResultType.Exception:

            var exceptionResult = result as ExceptionResult;
            if (exceptionResult != null  )
            {
              sb.AppendFormat("{0} EXCEPTION {1}{2}", exceptionResult.DateTimeUtc, exceptionResult.Summary, Environment.NewLine);
              sb.AppendFormat("Message: {0}\nStackTrace: {1}", exceptionResult.Exception.Message, exceptionResult.Exception.StackTrace);
            }
            break;

          case ResultType.Notify:

            var notifyResult = result as NotifyResult;
            if (notifyResult != null)
            {
              sb.AppendFormat("{0} NOTIFY {1}{2}", notifyResult.DateTimeUtc, notifyResult.Summary, Environment.NewLine);
              sb.AppendFormat("Message: {0}\nStackTrace: {1}", notifyResult.Exception.Message, notifyResult.Exception.StackTrace);
            }
            break;

          default:

            sb.AppendLine("Unhandled result type.");
            break;
        }
      }

      sb.AppendFormat("{0} {1} {0}", "************", "END");
      return sb.ToString();
    }
  }

  

}
