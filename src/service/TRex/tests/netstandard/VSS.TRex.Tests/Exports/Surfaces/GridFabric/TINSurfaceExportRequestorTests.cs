using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Exports.Surfaces;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Exports.Surfaces.Requestors;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridFabric
{
  public class TINSurfaceExportRequestorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<TINSurfaceRequestComputeFunc, TINSurfaceRequestArgument, TINSurfaceResult>();

    private void AddGridRouting()
    {
      AddApplicationGridRouting();
    }

    [Fact]
    public void Creation()
    {
      var req = new TINSurfaceExportRequestor();
      req.Should().NotBeNull();
    }

    // Test the executor does call in the the underlying request with a fail result from a empty/nonexistent model
    [Fact]
    public async Task Execute()
    {
      AddGridRouting();
      var req = new TINSurfaceExportRequestor();
      var result = await req.ExecuteAsync(new TINSurfaceRequestArgument());
      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.Unknown);
    }
  }
}
