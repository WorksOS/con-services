using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.Tests.DataSmoothing
{
  public static class DataSmoothingTestUtilities
  {
    public static void ConstructElevationSubGrid(GenericLeafSubGrid<float> subGrid, float elevation)
    {
      subGrid.ForEach((x, y) => subGrid.Items[x, y] = elevation);
    }

    public static GenericLeafSubGrid<float> ConstructElevationSubGrid(float elevation)
    {
      var subGrid = new GenericLeafSubGrid<float>
      {
        Level = SubGridTreeConsts.SubGridTreeLevels
      };
      subGrid.ForEach((x, y) => subGrid.Items[x, y] = elevation);

      return subGrid;
    }

    public static GenericSubGridTree<float, GenericLeafSubGrid<float>> ConstructSingleSubGridElevationSubGridTreeAtOrigin(float elevation)
    {
      var tree = new GenericSubGridTree<float, GenericLeafSubGrid<float>>();

      var subGrid = tree.ConstructPathToCell(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        SubGridPathConstructionType.CreateLeaf) as GenericLeafSubGrid<float>;
      ConstructElevationSubGrid(subGrid, elevation);

      return tree;
    }
  }
}
