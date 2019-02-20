using System.IO;

namespace VSS.TRex.Common.Utilities
{
  public class FilePathHelper
  {
    public static string EstablishLocalDesignFilepath(string projectUid)
    {
      var localPath = Path.Combine(new[] { TRexServerConfig.PersistentCacheStoreLocation, projectUid });
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
    public static string EstablishEmptyLocalExportFilepath(string projectUid, string filePath)
    {
      var localExportPath = Path.Combine(new[] { TRexServerConfig.PersistentCacheStoreLocation, projectUid, "Exports" });
      var localPath = Path.Combine(new[] { localExportPath, filePath });
      if (Directory.Exists(localPath))
        Directory.Delete(localPath, true);
      
      Directory.CreateDirectory(localPath);

      return localExportPath;
    }
  }
}
