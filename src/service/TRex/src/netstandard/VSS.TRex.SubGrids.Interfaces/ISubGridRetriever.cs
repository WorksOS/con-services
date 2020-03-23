using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids
{
  public interface ISubGridRetriever
  {
    ServerRequestResult RetrieveSubGrid(IClientLeafSubGrid clientGrid,
      SubGridTreeBitmapSubGridBits cellOverrideMask);
  }
}
