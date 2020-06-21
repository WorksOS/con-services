using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.SiteModels.Interfaces.Executors
{
  public interface ISiteModelRebuilderManager
  {
    int RebuildCount();

    IRebuildSiteModelMetaData[] GetRebuilersState();
  }
}
