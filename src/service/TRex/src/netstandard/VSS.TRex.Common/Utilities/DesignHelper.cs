using System.IO;

namespace VSS.TRex.Common.Utilities
{
  public class DesignHelper
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
  }
}
