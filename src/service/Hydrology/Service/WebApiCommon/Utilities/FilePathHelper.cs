using System;
using System.IO;

namespace VSS.Hydrology.WebApi.Common.Utilities
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
  }
}
