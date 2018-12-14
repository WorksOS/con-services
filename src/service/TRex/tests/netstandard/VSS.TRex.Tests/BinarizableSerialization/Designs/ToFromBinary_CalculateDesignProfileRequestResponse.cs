using System.Collections.Generic;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_CalculateDesignProfileResponse
  {
    [Fact]
    public void Test_CalculateDesignProfileResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CalculateDesignProfileResponse>("Empty CalculateDesignProfileResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_CalculateDesignProfileResponse()
    {
      var response = new CalculateDesignProfileResponse
      {
        Profile = new List<XYZS> {new XYZS(0, 0, 0, 0, 0), new XYZS(100, 101, 102, 103, 104) }
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom CalculateDesignProfileResponse not same after round trip serialisation");
    }
  }
}
