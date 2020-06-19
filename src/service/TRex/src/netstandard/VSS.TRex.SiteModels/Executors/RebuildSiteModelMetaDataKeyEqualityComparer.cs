using System;
using System.Collections.Generic;
using VSS.TRex.Common.Utilities;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels.Executors
{
  public class RebuildSiteModelMetaDataKeyEqualityComparer : IEqualityComparer<Guid>
  {
    public bool Equals(Guid x, Guid y)
    {
      if (ReferenceEquals(x, y))
        return true;

      return x != null && y != null && x.Equals(y);
    }

    public int GetHashCode(Guid obj) => GuidHashCode.Hash(obj);
  }
}
