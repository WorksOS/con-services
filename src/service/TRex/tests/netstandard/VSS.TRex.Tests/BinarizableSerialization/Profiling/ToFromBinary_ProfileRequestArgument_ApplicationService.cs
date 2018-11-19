using System;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Profiling
{
  public class ToFromBinary_ProfileRequestArgument_ApplicationService : IClassFixture<DILoggingFixture>, IClassFixture<AnalyticsTestsDIFixture>
  {
    private const double MIN_X = 171.7799;
    private const double MIN_Y = -41.838875;
    private const double MIN_Z = 3724.0;
    private const double MAX_X = 172.1234;
    private const double MAX_Y = -40.5678;
    private const double MAX_Z = 3724.0;
    private const double OFFSET = 0.0;

    [Fact]
    public void Test_ProfileRequestArgument_ApplicationService_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<ProfileRequestArgument_ApplicationService>("Empty ProfileRequestArgument_ApplicationService not same after round trip serialisation");
    }

    [Fact]
    public void Test_ProfileRequestArgument_ApplicationService()
    {
      var argument = new ProfileRequestArgument_ApplicationService()
      {
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesignID = Guid.NewGuid(),
        ProfileTypeRequired = GridDataType.Height,
        PositionsAreGrid = true,
        StartPoint = new WGS84Point(MIN_X, MIN_Y, MIN_Z),
        EndPoint = new WGS84Point(MAX_X, MAX_Y, MAX_Z),
        ReturnAllPassesAndLayers = false,
        DesignDescriptor = new DesignDescriptor()
        {
          DesignID = Guid.NewGuid(),
          FileName = "",
          Folder = "",
          Offset = OFFSET
        }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom ProfileRequestArgument_ApplicationService not same after round trip serialisation");
    }
  }
}
