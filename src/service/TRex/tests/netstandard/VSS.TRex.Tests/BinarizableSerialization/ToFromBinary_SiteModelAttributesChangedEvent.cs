using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.GridFabric.Events;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_SiteModelAttributesChangedEvent
  {
    [Fact]
    public void Test_SiteModelAttributesChangedEvent_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SiteModelAttributesChangedEvent>("Empty SiteModelAttributesChangedEvent not same after round trip serialisation");
    }

    [Fact]
    public void Test_SiteModelAttributesChangedEvent()
    {
      var argument = new SiteModelAttributesChangedEvent()
      {
        SiteModelID = Guid.NewGuid(),
        ExistenceMapModified = true,
        CsibModified = true,
        DesignsModified = true,
        SurveyedSurfacesModified = true,
        MachinesModified = true,
        MachineTargetValuesModified = true,
        MachineDesignsModified = true,
        ProofingRunsModified = true,
        ExistenceMapChangeMask = new byte[] { 1, 2, 3 }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom SiteModelAttributesChangedEvent not same after round trip serialisation");
    }
  }
}
