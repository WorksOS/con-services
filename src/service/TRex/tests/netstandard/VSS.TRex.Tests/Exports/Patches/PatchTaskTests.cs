using System;
using Moq;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Exports.Patches
{
  public class PatchTaskTests //: IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_PatchTask_Creation()
    {
      PatchTask task = new PatchTask(Guid.Empty, "", GridDataType.All);

      Assert.NotNull(task);
      Assert.NotNull(task.PatchSubGrids);
      Assert.True(0 == task.PatchSubGrids.Count, "Patched subgrid count not 0 after creation");
    }

    [Fact]
    public void Test_PatchTask_TransferResponse()
    {
      var pipeLine = new Mock<ISubGridPipelineBase>();
      pipeLine.Setup(mk => mk.Aborted).Returns(false);

      PatchTask task = new PatchTask(Guid.Empty, "", GridDataType.All)
      {
        PipeLine = pipeLine.Object
      };

      ClientHeightLeafSubGrid transferSubgrid = new ClientHeightLeafSubGrid(null, null, 0, 1, 0);

      task.TransferResponse(new IClientLeafSubGrid[] {transferSubgrid});

      Assert.True(task.PatchSubGrids.Count == 1,
        $"Count of transferred subgrids not 1 as expected (= {task.PatchSubGrids.Count}");
      Assert.True(task.PatchSubGrids[0] == transferSubgrid,
        $"Transferred subgrid is not the same as the one passed into the task.");
    }
  }
}
