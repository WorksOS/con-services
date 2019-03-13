using System;
using System.Collections.Generic;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Exports.Arguments

{
  public class ToFromBinary_CSVExportRequestArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CSVExportRequestArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CSVExportRequestArgument>("Empty CSVExportRequestArgument not same after round trip serialization");
    }

    [Fact]
    public void Test_CSVExportRequestArgument_Empty()
    {
      var request = new CSVExportRequestArgument() { };

      SimpleBinarizableInstanceTester.TestClass(request, "Empty CSVExportRequestArgument not same after round trip serialization");
    }

    [Fact]
    public void Test_CSVExportRequestArgument_WithContent()
    {
      var request = new CSVExportRequestArgument
      (
        Guid.NewGuid(),
        new FilterSet(new CombinedFilter()),
        "the filename",
        CoordType.LatLon,
        OutputTypes.PassCountAllPasses,
        new CSVExportUserPreferences(),
        new List<CSVExportMappedMachine>()
        {
          new CSVExportMappedMachine() {Uid = Guid.NewGuid(), InternalSiteModelMachineIndex = 0, Name = "Machine 1"},
          new CSVExportMappedMachine() {Uid = Guid.NewGuid(), InternalSiteModelMachineIndex = 1, Name = "Machine 2"},
          new CSVExportMappedMachine() {Uid = Guid.NewGuid(), InternalSiteModelMachineIndex = 2, Name = "Machine 3"}
        }, false, false
      );

      SimpleBinarizableInstanceTester.TestClass(request, "Empty CSVExportRequestArgument not same after round trip serialization");
    }
  }
}
