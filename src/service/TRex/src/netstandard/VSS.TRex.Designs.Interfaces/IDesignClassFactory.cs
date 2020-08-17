using System;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignClassFactory
  {
    IDesignBase NewInstance(Guid designUid, string fileName, double cellSize, Guid siteModelUid);
  }
}
