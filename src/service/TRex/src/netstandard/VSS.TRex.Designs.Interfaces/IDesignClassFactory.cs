using System;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignClassFactory
  {
    IDesignBase NewInstance(string fileName, double cellSize, Guid siteModelUid);
  }
}
