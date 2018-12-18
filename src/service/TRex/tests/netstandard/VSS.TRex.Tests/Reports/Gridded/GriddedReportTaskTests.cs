using System;
using Moq;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Reports.Gridded.Executors.Tasks;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Reports.Gridded
{
  public class GriddedReportTaskTests //: IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void GriddedReportTask_Creation()
    {
      var task = new GriddedReportTask(Guid.Empty, "", GridDataType.CellProfile);

      Assert.NotNull(task);
      Assert.NotNull(task.ResultantSubgrids);
      Assert.True(0 == task.ResultantSubgrids.Count, "Gridded report subgrid count not 0 after creation");
    }

    [Fact]
    public void GriddedReportTask_TransferResponse()
    {
      var pipeLine = new Mock<ISubGridPipelineBase>();
      pipeLine.Setup(mk => mk.Aborted).Returns(false);

      GriddedReportTask task = new GriddedReportTask(Guid.Empty, "", GridDataType.CellProfile)
      {
        PipeLine = pipeLine.Object
      };

      var transferSubgrid = new ClientCellProfileLeafSubgrid(null, null, 0, 1, 0);

      task.TransferResponse(new IClientLeafSubGrid[] {transferSubgrid});

      Assert.True(task.ResultantSubgrids.Count == 1,
        $"Count of transferred subgrids not 1 as expected (= {task.ResultantSubgrids.Count}");
      Assert.True(task.ResultantSubgrids[0] == transferSubgrid,
        $"Transferred subgrid is not the same as the one passed into the task.");
    }
  }
}
