using System;
using System.Collections.Generic;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;

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
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<CSVExportRequestComputeFunc, CSVExportRequestArgument, CSVExportRequestResponse>();
    
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
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var response = request.Execute(SimpleCSVExportRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
    }
  }
}


