using System;
using System.Globalization;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS
{
  /// <summary>
  /// Converts between the fullFilename stored by cws
  ///            and the filename component identifiable to the rest of the system
  ///  $"{projectTrn}||{DateTime.UtcNow}||{FileName}"
  ///    e.g. "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
  ///  Currently specific to calibration files (because of extension check)
  /// </summary>
  public class CwsFileNameHelper
  {
    /// <summary>
    /// Do we want to validate each time we get a component?
    /// </summary>
    public static bool ExtractComponents(string fullFileName, out string projectTrn, out string fileName, out DateTime fileDateUtc)
    {
      fileDateUtc = default;
      projectTrn = string.Empty;
      fileName = string.Empty;
      if (!string.IsNullOrEmpty(fullFileName))
      {
        var parts = fullFileName.Split(ProjectConfigurationModel.FilenamePathSeparator);
        if (parts.Length == 3)
        {
          projectTrn = parts[0].Trim();
          fileName = parts[2].Trim();
          if (!DateTime.TryParse(parts[1], out var fileDate))
            return false;

          fileDateUtc = fileDate;
          return true;
        }
      }

      return false;
    }

    public static string ExtractFileName(string fullFileName)
    {
      if (ExtractComponents(fullFileName, out _, out var fileName, out _))
        return fileName;

      return string.Empty;
    }

    public static DateTime? ExtractFileDate(string fullFileName)
    {
      if (ExtractComponents(fullFileName, out _, out var _, out var fileDateUtc))
        return fileDateUtc;

      return null;
    }

    public static string BuildFullFileName(string projectTrn, DateTime date, string fileName)
    {
      if (!string.IsNullOrEmpty(projectTrn) && !string.IsNullOrEmpty(fileName))
      {
        return $"{projectTrn}||{ date.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}||{fileName}";
      }
      return null;
    }

  }
}
