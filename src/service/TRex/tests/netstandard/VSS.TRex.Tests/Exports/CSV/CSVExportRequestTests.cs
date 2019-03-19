using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Reports.Gridded.Executors;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.Exports.CSV
{
  [UnitTestCoveredRequest(RequestType = typeof(CSVExportRequest))]
  public class CSVExportRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Test_CSVExportRequest_Creation()
    {
      var request = new CSVExportRequest();
      request.Should().NotBeNull();
    }
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting
      <CSVExportRequestComputeFunc, CSVExportRequestArgument, CSVExportRequestResponse>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting
      <CSVExportRequestComputeFunc, CSVExportRequestArgument, CSVExportRequestResponse>();


    //private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting
    //  <SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    private ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      return siteModel;
    }

    private CSVExportRequestArgument SimpleCSVExportRequestArgument(Guid projectUid)
    {
      return new CSVExportRequestArgument
      { 
        FileName = "the file name",
        Filters = new FilterSet(new CombinedFilter()),
        CoordType = CoordType.Northeast,
        OutputType = OutputTypes.PassCountLastPass,
        UserPreferences = new CSVExportUserPreferences(),
        MappedMachines = new List<CSVExportMappedMachine>(),
        RestrictOutputSize = false,
        RawDataAsDBase = false,
        TRexNodeID = "'Test_CSVExportRequest_Execute_EmptySiteModel",
        ProjectID = projectUid
      };
    }

    [Fact]
    public void Test_CSVExportRequest_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();
      var siteModel = NewEmptyModel();
      var request = new CSVExportRequest();
      var response = request.Execute(SimpleCSVExportRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
    }

    [Fact]
    public void Test_CSVExportRequest_Execute_SingleCellSiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      BuildModelForSingleCell(out var siteModel, 0.5f);

      var arg = SimpleCSVExportRequestArgument(siteModel.ID);
      var request = new CSVExportRequest();
      var response = request.Execute(arg);

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);
      response.fileName.Should().Be(arg.FileName);
    }

    private void BuildModelForSingleCell(out ISiteModel siteModel, float heightIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

      siteModel = NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = baseHeight + x * heightIncrement,
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
    }
  }
}


