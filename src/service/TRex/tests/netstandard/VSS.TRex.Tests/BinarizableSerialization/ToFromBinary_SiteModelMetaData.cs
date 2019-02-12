using System;
using FluentAssertions;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_SiteModelMetaData
  {
    [Fact]
    public void Test_SiteModelMetadata_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SiteModelMetadata>("Empty SiteModelMetadata not same after round trip serialisation");
    }

    [Fact]
    public void Test_SiteModelMetadata()
    {
      var argument = new SiteModelMetadata
      {
        ID = Guid.NewGuid(),
        SiteModelExtent = BoundingWorldExtent3D.Full(),
        CreationDate = DateTime.UtcNow,
        LastModifiedDate = DateTime.UtcNow,
        MachineCount = 10,
        DesignCount = 5,
        SurveyedSurfaceCount = 3,
        AlignmentCount = 1
      };

      var result = SimpleBinarizableInstanceTester.TestClass(argument, "Custom SiteModelMetadata not same after round trip serialisation");

      argument.LastModifiedDate.Should().Be(result.member.LastModifiedDate, "Dates are not equal");
    }
  }
}
