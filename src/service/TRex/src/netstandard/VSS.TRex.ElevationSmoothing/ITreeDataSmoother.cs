using VSS.TRex.SubGridTrees;

namespace VSS.TRex.DataSmoothing
{
  public interface ITreeDataSmoother<TV> : IDataSmoother
  {
    GenericSubGridTree<TV, GenericLeafSubGrid<TV>> Smooth(GenericSubGridTree<TV, GenericLeafSubGrid<TV>> source);
  }
}
