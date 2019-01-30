using System;
using System.ComponentModel;

namespace VSS.Common.Abstractions.ExtensionMethods
{
  public static class EnumExtensionMethods
  {
    public static string GetDescription(this Enum value)
    {
      var type = value.GetType();

      var memInfo = type.GetMember(value.ToString());

      if (memInfo.Length <= 0) 
        return value.ToString();

      var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
      return attrs.Length > 0 
        ? ((DescriptionAttribute)attrs[0]).Description 
        : value.ToString();
    }
  }
}