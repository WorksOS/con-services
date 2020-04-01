using System;
using System.ComponentModel;

namespace VSS.Common.Abstractions.Enums
{
  public static class ExtensionMethods
  {
    /// <summary>
    /// Checks an enum to see if it has a [Description] Enum, and retrieves that value
    /// If it doesn't exist, returns the enum.ToString()
    /// See my comment here: https://stackoverflow.com/questions/1923987/retrieve-value-of-enum-based-on-index-c-sharp/1924056#1924056 
    /// </summary>
    public static string GetDescription(this Enum value)
    {
      var type = value.GetType();

      var memInfo = type.GetMember(value.ToString());

      if (memInfo.Length > 0)
      {
        var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attrs.Length > 0)
          return ((DescriptionAttribute)attrs[0]).Description;
      }

      return value.ToString();
    }
  }
}