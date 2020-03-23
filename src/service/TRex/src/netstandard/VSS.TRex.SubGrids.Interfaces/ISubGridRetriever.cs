using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids.Interfaces
{
  public interface ISubGridRetriever
  {
    ServerRequestResult RetrieveSubGrid(IClientLeafSubGrid clientGrid,
      SubGridTreeBitmapSubGridBits cellOverrideMask);
  }
}
