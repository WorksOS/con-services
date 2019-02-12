﻿using System;
using System.Collections.Generic;
using VSS.TRex.Filters;
using Xunit;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.Productivity3D.Models.Enums;

namespace VSS.TRex.Tests.BinarizableSerialization.Exports.Arguments

{
  public class ToFromBinary_CSVExportRequestArgument : BaseTests, IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CSVExportRequestArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CSVExportRequestArgument>("Empty CSVExportRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_CSVExportRequestArgument_Empty()
    {
      var request = new CSVExportRequestArgument() { };

      SimpleBinarizableInstanceTester.TestClass(request, "Empty CSVExportRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_CSVExportRequestArgument_WithContent()
    {
      var request = new CSVExportRequestArgument
      (
        Guid.NewGuid(),
        new FilterSet(new CombinedFilter()),
        CoordType.LatLon,
        OutputTypes.PassCountAllPasses,
        new List<CSVExportMappedMachine>()
        {
          new CSVExportMappedMachine() {Uid = Guid.NewGuid(), InternalSiteModelMachineIndex = 0, Name = "Machine 1"},
          new CSVExportMappedMachine() {Uid = Guid.NewGuid(), InternalSiteModelMachineIndex = 1, Name = "Machine 2"},
          new CSVExportMappedMachine() {Uid = Guid.NewGuid(), InternalSiteModelMachineIndex = 2, Name = "Machine 3"}
        }
      );

      SimpleBinarizableInstanceTester.TestClass(request, "Empty CSVExportRequestArgument not same after round trip serialisation");
    }
  }
}
