using System;
using System.IO;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  public static class ImportedFileUtils
  {

    // NhOp includes surveyedUtc/s in name, but Project does not. Samples:
    // JB topo southern motorway_2010-11-29T153300Z.TTM   SS=2010-11-29 15:33:00.0000000
    // Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM ssUtc=2016-08-16 00:37:24.0000000
    public static string IncludeSurveyedUtcInName(string name, DateTime surveyedUtc)
    {
      //Note: ':' is an invalid character for filenames in Windows so get rid of them
      // There is a need to potentially suffix a date a 2nd time, so don't check if one exists.
      return Path.GetFileNameWithoutExtension(name) +
             "_" + surveyedUtc.ToIso8601DateTimeString().Replace(":", string.Empty) +
             Path.GetExtension(name);
    }

    public static string RemoveSurveyedUtcFromName(string name)
    {
      var shortFileName = Path.GetFileNameWithoutExtension(name);
      var format = "yyyy-MM-ddTHHmmssZ";
      if (!DoesNameIncludeUtc(shortFileName, format))
        return name;
      return shortFileName.Substring(0, shortFileName.Length - format.Length - 1) + Path.GetExtension(name);
    }

    private static bool DoesNameIncludeUtc(string name, string format)
    {
      if (name.Length <= format.Length)
        return false;
      var endOfName = name.Substring(name.Length - format.Length);
      var pattern = "^\\d{4}-\\d{2}-\\d{2}T\\d{6}Z$";
      var isMatch = (System.Text.RegularExpressions.Regex.IsMatch(endOfName, pattern,
        System.Text.RegularExpressions.RegexOptions.IgnoreCase)) ;
      return isMatch;
    }

  }
}
