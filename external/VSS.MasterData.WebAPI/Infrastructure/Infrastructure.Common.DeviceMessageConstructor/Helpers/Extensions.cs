using System;

namespace Infrastructure.Common.DeviceMessageConstructor.Helpers
{
   public static class Extensions
   {
      public static string ToStringWithoutHyphens(this Guid guid)
      {
         return guid.ToString("N");
      }

      public static string ToStringWithoutHyphens(this string guid)
      {
         return new Guid(guid).ToString("N");
      }

      public static string WrapWithUnhex(this string val)
      {
         return string.Format("UNHEX('{0}')", val);
      }
   }
}
