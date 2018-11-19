﻿using System;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.Tests.TestFixtures;
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
      var argument = new SiteModelMetadata()
      {
        ID = Guid.NewGuid(),
        Name = "Site Model",
        Description = "Test site model",
        SiteModelExtent = BoundingWorldExtent3D.Full(),
        LastModifiedDate = DateTime.UtcNow,
        MachineCount = 10,
        DesignCount = 5,
        SurveyedSurfaceCount = 3
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom SiteModelMetadata not same after round trip serialisation");
    }
  }
}
