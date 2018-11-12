using System;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Surfaces
{
  public class ToFromBinary_TINSurfaceArgument : IClassFixture<DILoggingFixture>, IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_TINSurfaceRequestArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<TINSurfaceRequestArgument>("Empty TINSurfaceRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_TINSurfaceRequestArgument()
    {
      var argument = new TINSurfaceRequestArgument()
      {
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        Tolerance = 0.1
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom TINSurfaceRequestArgument not same after round trip serialisation");
    }
  }
}
