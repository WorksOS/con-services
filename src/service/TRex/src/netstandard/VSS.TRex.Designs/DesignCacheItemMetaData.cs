using System;
using VSS.TRex.Designs.Interfaces;

namespace VSS.TRex.Designs
{
  public class DesignCacheItemMetaData
  {
    public IDesignBase Design;
    public long SizeInCache;
    public DateTime LastTouchedDate;

    public DesignCacheItemMetaData(IDesignBase design, long sizeInCache)
    {
      Design = design;
      SizeInCache = sizeInCache;
      LastTouchedDate = DateTime.UtcNow;
    }

    public void Touch()
    {
      LastTouchedDate = DateTime.UtcNow;
    }
  }
}
