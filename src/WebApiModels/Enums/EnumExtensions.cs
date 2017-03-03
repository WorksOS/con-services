using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace WebApiModels.Enums
{
  public static class EnumExtensions
  {
    public static string Description(this Enum value)
    {
      // get attributes  
      var field = value.GetType().GetField(value.ToString());
      var attributes = field.GetCustomAttributes(false);

      // Description is in a hidden Attribute class called DisplayAttribute
      // Not to be confused with DisplayNameAttribute
      dynamic displayAttribute = null;

      if (attributes.Any())
      {
        displayAttribute = attributes.ElementAt(0);
      }

      // return description
      return displayAttribute?.Description ?? "Description Not Found";
    }
  }
}
