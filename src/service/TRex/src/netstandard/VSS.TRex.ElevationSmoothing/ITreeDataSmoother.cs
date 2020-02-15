using VSS.TRex.SubGridTrees;

namespace VSS.TRex.ElevationSmoothing
{
  public interface ITreeDataSmoother<TV> : IDataSmoother
  {
    GenericSubGridTree<TV, GenericLeafSubGrid<TV>> Smooth();
  }
}
