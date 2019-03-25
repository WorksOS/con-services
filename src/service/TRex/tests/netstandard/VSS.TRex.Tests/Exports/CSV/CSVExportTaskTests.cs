using System.Collections.Generic;
using FluentAssertions;
using Moq;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportTaskTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var task = new CSVExportTask();
      task.Should().NotBeNull();
      task.DataRows.Should().NotBeNull();
      task.SubGridExportProcessor.Should().BeNull();
    }

    [Fact]
    public void Test_PatchTask_TransferResponse_FailWithNoSubGrids()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var pipeLine = new Mock<ISubGridPipelineBase>();
      pipeLine.Setup(mk => mk.Aborted).Returns(false);

      var task = new CSVExportTask
      {
        PipeLine = pipeLine.Object,
        SubGridExportProcessor = new CSVExportSubGridProcessor(new CSVExportRequestArgument(siteModel.ID, new FilterSet(new CombinedFilter()), "", CoordType.Northeast, OutputTypes.PassCountLastPass, new CSVExportUserPreferences(), new List<CSVExportMappedMachine>(), false, false))
      };

      task.TransferResponse(new IClientLeafSubGrid[] { }).Should().BeFalse();
    }
  }
}
