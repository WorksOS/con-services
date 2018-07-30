using VSS.TRex.Exports.Surfaces;
using VSS.TRex.SubGridTrees;
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
  }
}
