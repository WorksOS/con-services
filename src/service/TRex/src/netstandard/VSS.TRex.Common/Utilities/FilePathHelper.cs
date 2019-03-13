using System;
using System.IO;

namespace VSS.TRex.Common.Utilities
{
  public class FilePathHelper
  {
    public static string GetTempFolderForProject(Guid projectUid)
    {
      var localPath = Path.Combine(new[] { Path.GetTempPath(), projectUid.ToString() });
      if (!Directory.Exists(localPath))
      {
        Directory.CreateDirectory(localPath);
      }

      return localPath;
    }

    /// <summary>
    /// Create an empty directory: projectUid\Exports\FilePath
    ///    under which to store the file/files
    /// Note that FilePath includes Filename and unique TRexID
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetTempFolderForExport(Guid projectUid, string filePath)
    {
      var localExportPath = Path.Combine(new[] { GetTempFolderForProject(projectUid), "Exports" });
      var localPath = Path.Combine(new[] { localExportPath, filePath });
      if (!Directory.Exists(localPath))
        Directory.CreateDirectory(localPath);

      return localExportPath;
    }
  }
}
