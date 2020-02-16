using System.Globalization;
using System.Reflection;
using System.Resources;

namespace VSS.Hosted.VLCommon.Resources
{
  public static class VLResourceManager
  {
    public static string GetString(string key, string culture)
    {
      return GetString(key, new CultureInfo(culture));
    }

    public static string GetString(string key, CultureInfo culture)
    {      
      return rm.GetString(key, culture);
    }

    public static object GetObject(string key)
    {
      return rm.GetObject(key);
    }

    private static ResourceManager rm = new ResourceManager("VSS.Hosted.VLCommon.Resources.NHSvr", Assembly.GetExecutingAssembly());

  }
}
