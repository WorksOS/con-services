using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.CellDatum.GridFabric
{
  public class CellDatumRequestComputeFuncTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_CellDatumRequestComputeFunc_ApplicationService_Creation()
    {
      CellDatumRequestComputeFunc_ApplicationService func = new CellDatumRequestComputeFunc_ApplicationService();

      Assert.NotNull(func);
    }
    [Fact]
    public void Test_CellDatumRequestComputeFunc_ClusterCompute_Creation()
    {
      CellDatumRequestComputeFunc_ClusterCompute func = new CellDatumRequestComputeFunc_ClusterCompute();

      Assert.NotNull(func);
    }
  }
}
