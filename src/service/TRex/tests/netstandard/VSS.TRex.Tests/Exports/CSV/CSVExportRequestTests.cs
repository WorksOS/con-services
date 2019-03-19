using System;
using System.Collections.Generic;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.Exports.CSV
{
  [UnitTestCoveredRequest(RequestType = typeof(CSVExportRequest))]
  public class CSVExportRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<CSVExportRequestComputeFunc, CSVExportRequestArgument, CSVExportRequestResponse>();
    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    [Fact]
    public void Test_CSVExportRequest_Creation()
    {
      var request = new CSVExportRequest();
      request.Should().NotBeNull();
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
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var response = request.Execute(SimpleCSVExportRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public void Test_CSVExportRequest_Execute_SingleCellSinglePass()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();

      var baseDate = new DateTime(2000, 1, 1, 1, 0, 0, 0);

      var cellPasses = new []
      {
        new CellPass
        {
          Time = baseDate,
          Height = 1.0f
        }
      };

      DITAGFileAndSubGridRequestsWithIgniteFixture.AddSingleCellWithPasses(siteModel,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      var response = request.Execute(SimpleCSVExportRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.Unknown);
    }
  }
}


