using VSS.TRex.Exports.Surfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Utilities;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
{
    public class GridToTINDecimatorTests
    {
      [Fact]
      public void GridToTINDecimatorTests_Creation_NoDataStore()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(null);

        Assert.NotNull(decimator);
      }

      [Fact]
      public void GridToTINDecimatorTests_Creation_WithDataStore()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new GenericSubGridTree<float>());

        Assert.NotNull(decimator);
      }

      [Fact]
      public void GridToTINDecimatorTests_Refresh()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new GenericSubGridTree<float>());
        decimator.Refresh();

        Assert.NotNull(decimator);
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_EmptyDataSource()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new GenericSubGridTree<float>());
        bool result = decimator.BuildMesh();

        Assert.False(result, $"Failed to fail to build build mesh from empty data store with fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_SetDecimationExtents()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(null);
        decimator.SetDecimationExtents( new BoundingWorldExtent3D(0, 0, 100, 100));

        Assert.True(decimator.GridCalcExtents.MinX == 0);
        Assert.True(decimator.GridCalcExtents.MaxX == 100);
        Assert.True(decimator.GridCalcExtents.MinY == 0);
        Assert.True(decimator.GridCalcExtents.MaxY == 100);
      }

      private BoundingWorldExtent3D DataStoreExtents(GenericSubGridTree<float> dataStore)
      {
        BoundingWorldExtent3D ComputedGridExtent = BoundingWorldExtent3D.Inverted();

        dataStore.ScanAllSubGrids(subGrid =>
        {
          SubGridUtilities.SubGridDimensionalIterator((x, y) =>
          {
            float elev = ((GenericLeafSubGrid<float>) subGrid).Items[x, y];
            if (elev != Common.Consts.NullHeight)
              ComputedGridExtent.Include((int)(subGrid.OriginX + x), (int)(subGrid.OriginY + y), elev);
          });

          return true;
        });

        if (ComputedGridExtent.IsValidPlanExtent)
          ComputedGridExtent.Offset(-(int)SubGridTree.DefaultIndexOriginOffset, -(int)SubGridTree.DefaultIndexOriginOffset);

        // Convert the grid rectangle to a world rectangle
        BoundingWorldExtent3D ComputedWorldExtent = new BoundingWorldExtent3D(ComputedGridExtent.MinX * dataStore.CellSize,
          ComputedGridExtent.MinY * dataStore.CellSize,
          (ComputedGridExtent.MaxX + 1) * dataStore.CellSize,
          (ComputedGridExtent.MaxY + 1) * dataStore.CellSize,
          ComputedGridExtent.MinZ, ComputedGridExtent.MaxZ);

        return ComputedWorldExtent;
      }

    [Fact]
      public void GridToTINDecimatorTests_BuildMesh_SinglePoint()
      {
        GenericSubGridTree<float> dataStore = new GenericSubGridTree<float>();
        dataStore[100, 100] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);

        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.False(result, $"Failed to fail to build build mesh from data store with single point fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_TwoPoints()
      {
        GenericSubGridTree<float> dataStore = new GenericSubGridTree<float>();
        dataStore[100, 100] = 100.0f;
        dataStore[200, 200] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.False(result, $"Failed to fail to build build mesh from data store with two points fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_ThreePoints()
      {
        GenericSubGridTree<float> dataStore = new GenericSubGridTree<float>();
        dataStore[100, 100] = 100.0f;
        dataStore[101, 101] = 100.0f;
        dataStore[101, 100] = 100.0f;

      GridToTINDecimator decimator = new GridToTINDecimator(dataStore);
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.True(result, $"Failed to build build mesh from data store with three points fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_GetTIN()
      {
        GenericSubGridTree<float> dataStore = new GenericSubGridTree<float>();
        dataStore[100, 100] = 100.0f;
        dataStore[101, 101] = 100.0f;
        dataStore[101, 100] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.True(result, $"Failed to build build mesh from data store with three points fault code {decimator.BuildMeshFaultCode}");

        Assert.NotNull(decimator.GetTIN());
        Assert.True(decimator.GetTIN().Triangles.Count == 1);
        Assert.True(decimator.GetTIN().Triangles[0].Vertices[0].Z == 100f);
        Assert.True(decimator.GetTIN().Triangles[0].Vertices[1].Z == 100f);
        Assert.True(decimator.GetTIN().Triangles[0].Vertices[2].Z == 100f);

        Assert.True(decimator.GetTIN().Vertices.Count == 3);
        Assert.True(decimator.GetTIN().Vertices[0].Z == 100.0f);
        Assert.True(decimator.GetTIN().Vertices[1].Z == 100.0f);
        Assert.True(decimator.GetTIN().Vertices[2].Z == 100.0f);

        decimator.GetTIN().SaveToFile(@"C:\temp\UnitTestExportTTM.ttm", true);
    }
  }
}
