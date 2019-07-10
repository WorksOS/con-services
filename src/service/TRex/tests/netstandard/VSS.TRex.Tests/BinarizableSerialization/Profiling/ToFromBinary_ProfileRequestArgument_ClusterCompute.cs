using System;
using VSS.TRex.Common.Records;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Profiling
{
  public class ToFromBinary_ProfileRequestArgument_ClusterCompute : IClassFixture<DILoggingFixture>, IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_ProfileRequestArgument_ClusterCompute_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<ProfileRequestArgument_ClusterCompute>("Empty ProfileRequestArgument_ClusterCompute not same after round trip serialisation");
    }

    [Fact]
    public void Test_ProfileRequestArgument_ClusterCompute()
    {
      var coords = new XYZ[3];
      for (var i = 0; i < coords.Length; i++) 
        coords[i] = new XYZ(0.0, 0.0, 0.0);

      var argument = new ProfileRequestArgument_ClusterCompute()
      {
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = new DesignOffset(Guid.NewGuid(), 1.5),
        ProfileTypeRequired = GridDataType.Height,
        NEECoords = coords,
        ReturnAllPassesAndLayers = false,
        Overrides = new OverrideParameters { CMVRange = new CMVRangePercentageRecord(35.7, 104.9), OverrideMachineCCV = true, OverridingMachineCCV = 142 }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom ProfileRequestArgument_ClusterCompute not same after round trip serialisation");
    }
  }
}
