using System.Collections.Generic;
using VSS.TRex.Geometry;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelDesignList : IList<ISiteModelDesign>
  {
    ISiteModelDesign CreateNew(string name, BoundingWorldExtent3D extents);

    int IndexOf(string designName);

    /// <summary>
    /// Indexer supporting locating designs by the design name
    /// </summary>
    /// <param name="designName"></param>
    /// <returns></returns>
    ISiteModelDesign this[string designName] { get; }
  }
}
