using System;
using System.IO;

namespace VSS.Common.Abstractions.Extensions
{
  public static class StringExtensions
  {
    /// <summary>
    /// Add surveyed UTC to file name. 
    /// </summary>
    public static string IncludeSurveyedUtcInName(this string fileName, DateTime surveyedUtc)
    {
      //Note: ':' is an invalid character for file names in Windows so get rid of them
      // There is a need to potentially suffix a date a 2nd time, so don't check if one exists.
      return Path.GetFileNameWithoutExtension(fileName) +
             "_" + surveyedUtc.ToIso8601DateTimeString().Replace(":", string.Empty) +
             Path.GetExtension(fileName);
    }
  }
}
