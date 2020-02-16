using System;
using System.Text.RegularExpressions;

namespace VSS.Hosted.VLCommon.Utilities
{
  public class SiteUtilities
  {
    private static string PlainName(string siteName)
    {
      string baseName;
      //Regular expression to match ISO8601 DateTime i.e. yyyy-MM-ddTHH:mm:ssZ 
      Regex regex = new Regex(@"(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z)$");

      Match match = regex.Match(siteName);
      if (match.Success)
      {
        baseName = siteName.Substring(0, match.Index - 1);
      }
      else
      {
        baseName = siteName;
      }
      return baseName;
    }

    public static string DecoratedName(string siteName, DateTime updateUTC)
    {
      return PlainName(siteName) + " " + updateUTC.ToIso8601DateTimeString();
    }
  }
}
