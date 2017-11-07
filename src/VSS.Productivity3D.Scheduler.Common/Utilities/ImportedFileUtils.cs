using System;

namespace VSS.Productivity3D.Scheduler.Common.Utilities
{
  public static class ImportedFileUtils
  {

    // NhOp includes surveyedUtc/s in name, but Project does not. Samples:
    // JB topo southern motorway_2010-11 - 29T153300Z.TTM   SS=2010-11-29 15:33:00.0000000
    // Aerial Survey 120819_2012 - 08 - 19T035400Z_2016 - 08 - 16T003724Z.TTM ssUtc=2016-08-16 00:37:24.0000000
    public static string IncludeSurveyedUtcInName(string name, DateTime surveyedUtc)
    {
      // may already have a survey date in it, append new one
      var nameParts = name.Split('.');
      if (nameParts.Length != 2)
        return String.Empty;

      string newName =
        string.Format(
          format:
          $"{nameParts[0]}_{surveyedUtc.Year:0000}-{surveyedUtc.Month:00}-{surveyedUtc.Day:00}T{surveyedUtc.Hour:00}{surveyedUtc.Minute:00}{surveyedUtc.Second:00}Z.{nameParts[1]}");
      return newName;
    }

    public static string RemoveSurveyedUtcFromName(string name)
    {
      var indexOfExtention = name.IndexOf('.');
      var indexOfUnderScore = name.IndexOf('_');
      if (indexOfUnderScore == -1)
        return name;
      string newName = name.Substring(0, indexOfUnderScore) + name.Substring(indexOfExtention);
      return newName;
    }

  }
}
