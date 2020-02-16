using System;

namespace VSS.Hosted.VLCommon
{
    public static class ObjectExtentions
    {   
        //
        // Summary:
        //     work even if o is null.
        //
        // Returns:
        //     if o is null it returns null
        public static string ToNullString(this Object ob)
        {
            try
            {
                return ob.ToString();
            }
            catch
            {
                return "null";
            }

        }

        //
        // Summary:
        //     work even if object is null.
        //
        // Returns:
        //     if object is null it returns empty string
        public static string getString(this Object obj)
        {
          if (obj == null)
          {
            return String.Empty;
          }

          return obj.ToString();

        }

        //
        // Summary:
        //     work even if object is null.
        //
        // Returns:
        //     if object is null it returns 0
        public static double getDouble(this Object obj)
        {
          if (obj == null)
          {
            return 0;
          }
          return (double)obj;
        }

        //
        // Summary:
        //     work even if object is null.
        //
        // Returns:
        //     if object is null it returns 0
        public static decimal getDecimal(this Object obj)
        {
          if (obj == null)
          {
            return 0;
          }
          return (decimal)obj;
        }

        //
        // Summary:
        //     work even if object is null.
        //
        // Returns:
        //     if object is null it returns empty
        public static string getEnumString<T>(this object obj) where T : struct
        {
          if (obj == null)
            return string.Empty;
          
          string enumTokenName = obj.ToString();

          if (string.IsNullOrEmpty(enumTokenName.ToString()))
            return string.Empty;

          return enumTokenName.ToEnum<T>().ToString();
        }
    }
}
