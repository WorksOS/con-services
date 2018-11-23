using System.IO;
using VSS.TRex.Common;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public class DesignControllerHelper
  {
    public static string EstablishLocalDesignFilepath(string projectUid)
    {
      var localPath = Path.Combine(new[] { TRexServerConfig.PersistentCacheStoreLocation, projectUid });
      if (!System.IO.Directory.Exists(localPath))
      {
        System.IO.Directory.CreateDirectory(localPath);
      }

      return localPath;
    }
  }
}
