using VSS.TRex.Exports.Surfaces.GridDecimator;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
  public class DecimationElevationSubGridTree : GenericSubGridTree<float>
  {
    public override float NullCellValue => Common.Consts.NullHeight;
  }
  
  public class GridToTINDecimatorTests : IClassFixture<DILoggingFixture>
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
        GridToTINDecimator decimator = new GridToTINDecimator(new DecimationElevationSubGridTree());

        Assert.NotNull(decimator);
      }

      [Fact]
      public void GridToTINDecimatorTests_Refresh()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new DecimationElevationSubGridTree());
        decimator.Refresh();

        Assert.NotNull(decimator);
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_EmptyDataSource()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new DecimationElevationSubGridTree());
        bool result = decimator.BuildMesh();

        Assert.False(result, $"Failed to fail to build mesh from empty data store with fault code {decimator.BuildMeshFaultCode}");
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

      private BoundingWorldExtent3D DataStoreExtents(DecimationElevationSubGridTree dataStore)
      {
        BoundingWorldExtent3D ComputedGridExtent = BoundingWorldExtent3D.Inverted();

        dataStore.ScanAllSubGrids(subGrid =>
        {
          SubGridUtilities.SubGridDimensionalIterator((x, y) =>
          {
            float elev = ((GenericLeafSubGrid<float>) subGrid).Items[x, y];
            if (elev != 0)
              ComputedGridExtent.Include((int)(subGrid.OriginX + x), (int)(subGrid.OriginY + y), elev);
            else
              ((GenericLeafSubGrid<float>)subGrid).Items[x, y] = Common.Consts.NullHeight;
          });

          return true;
        });

        if (ComputedGridExtent.IsValidPlanExtent)
          ComputedGridExtent.Offset(-(int)SubGridTree.DefaultIndexOriginOffset, -(int)SubGridTree.DefaultIndexOriginOffset);

        // Convert the grid rectangle to a world rectangle
        BoundingWorldExtent3D ComputedWorldExtent = new BoundingWorldExtent3D
         (ComputedGridExtent.MinX - 0.01 * dataStore.CellSize,
          ComputedGridExtent.MinY - 0.01 * dataStore.CellSize,
          (ComputedGridExtent.MaxX + 1 + 0.01) * dataStore.CellSize,
          (ComputedGridExtent.MaxY + 1 + 0.01) * dataStore.CellSize,
          ComputedGridExtent.MinZ, ComputedGridExtent.MaxZ);

      return ComputedWorldExtent;
      }

    [Fact]
      public void GridToTINDecimatorTests_BuildMesh_SinglePoint()
      {
        DecimationElevationSubGridTree dataStore = new DecimationElevationSubGridTree();
        dataStore[SubGridTree.DefaultIndexOriginOffset + 100, SubGridTree.DefaultIndexOriginOffset + 100] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);

        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.False(result, $"Failed to fail to build mesh from data store with single point fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_TwoPoints()
      {
        DecimationElevationSubGridTree dataStore = new DecimationElevationSubGridTree();
        dataStore[SubGridTree.DefaultIndexOriginOffset + 100, SubGridTree.DefaultIndexOriginOffset + 100] = 100.0f;
        dataStore[SubGridTree.DefaultIndexOriginOffset + 101, SubGridTree.DefaultIndexOriginOffset + 101] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.False(result, $"Failed to fail to build mesh from data store with two points fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_ThreePoints()
      {
        DecimationElevationSubGridTree dataStore = new DecimationElevationSubGridTree();
        dataStore[SubGridTree.DefaultIndexOriginOffset + 100, SubGridTree.DefaultIndexOriginOffset + 100] = 100.0f;
        dataStore[SubGridTree.DefaultIndexOriginOffset + 101, SubGridTree.DefaultIndexOriginOffset + 101] = 100.0f;
        dataStore[SubGridTree.DefaultIndexOriginOffset + 101, SubGridTree.DefaultIndexOriginOffset + 100] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.True(result, $"Failed to build mesh from data store with three points fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_GetTIN()
      {
        DecimationElevationSubGridTree dataStore = new DecimationElevationSubGridTree();

        SubGridUtilities.SubGridDimensionalIterator((x, y) => dataStore[SubGridTree.DefaultIndexOriginOffset + x, SubGridTree.DefaultIndexOriginOffset + y] = 100.0f);

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.True(result, $"Failed to build mesh from data store with a subgrid of points fault code {decimator.BuildMeshFaultCode}");

        Assert.NotNull(decimator.GetTIN());

//        string fileName = $@"C:\temp\UnitTestExportTTM({DateTime.Now.Ticks}).ttm";
//        decimator.GetTIN().SaveToFile(fileName, true);  

//        TrimbleTINModel tin = new TrimbleTINModel();        
//        tin.LoadFromFile(fileName);

        Assert.True(decimator.GetTIN().Triangles.Count == 3);
        Assert.True(decimator.GetTIN().Triangles[0].Vertices[0].Z == 100f);
        Assert.True(decimator.GetTIN().Triangles[0].Vertices[1].Z == 100f);
        Assert.True(decimator.GetTIN().Triangles[0].Vertices[2].Z == 100f);

        Assert.True(decimator.GetTIN().Vertices.Count == 3);
        Assert.True(decimator.GetTIN().Vertices[0].Z == 100.0f);
        Assert.True(decimator.GetTIN().Vertices[1].Z == 100.0f);
        Assert.True(decimator.GetTIN().Vertices[2].Z == 100.0f);
    }
  }
}
