using FluentAssertions;
using Moq;
using VSS.TRex.Exports.Surfaces.Executors.Tasks;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
{
  public class SurfaceTaskTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var task = new SurfaceTask();
      task.Should().NotBeNull();
    }

    [Fact]
    public void TransferResponse_FailWithNoSubGrids()
    {
      var pipeLine = new Mock<ISubGridPipelineBase>();
      pipeLine.Setup(mk => mk.Aborted).Returns(false);

      var task = new SurfaceTask
      {
        PipeLine = pipeLine.Object
      };

      task.TransferResponse(new IClientLeafSubGrid[] { }).Should().BeFalse();
    }
  }
}
