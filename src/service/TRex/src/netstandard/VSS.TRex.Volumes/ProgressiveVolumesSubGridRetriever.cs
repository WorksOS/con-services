using System;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Volumes
{
  /*
   * What filter aspects need to be adhered to? Pass count filtering?
   * Does minimum elevation mode need to be respected [Yes]
   *
   * Should always have a time range?
   *
   */
  public class ProgressiveVolumesSubGridRetriever : ISubGridRetriever
  {
    public ServerRequestResult RetrieveSubGrid(IClientLeafSubGrid clientGrid, SubGridTreeBitmapSubGridBits cellOverrideMask)
    {
      throw new NotImplementedException();
    }
  }
}
