using System;
using System.IO;

namespace VSS.TRex.Common.Utilities
{
  public class DesignHelper
  {
    public static string EstablishLocalDesignFilepath(Guid projectUid)
    {
      var localPath = Path.Combine(Path.GetTempPath(), "TRexCache", projectUid.ToString());
      if (!Directory.Exists(localPath))
      {
        Directory.CreateDirectory(localPath);
      }

      return localPath;
    }
  }
}
