using System;

namespace VSS.TRex.SiteModels.Interfaces.Executors
{
  [Flags]
  public enum RebuildSiteModelFlags : byte
  {
    AddProcessedTagFileToArchive = 1
  }
}
