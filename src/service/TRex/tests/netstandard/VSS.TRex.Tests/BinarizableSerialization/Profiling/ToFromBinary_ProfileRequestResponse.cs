using System.Collections.Generic;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Profiling
{
  public class ToFromBinary_ProfileRequestResponse
  {
    [Fact]
    public void Test_ProfileRequestArgument_ProfileRequestResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<ProfileRequestResponse>("Empty ProfileRequestResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_ProfileRequestResponse()
    {
      var cell = new ProfileCell(new FilteredMultiplePassInfo
        {
          PassCount = 1,
          FilteredPassData = new [] { new FilteredPassData() }
        },
        1, 2, 3.0, 4.0);

      var cells = new List<ProfileCell>{ cell };

      var response = new ProfileRequestResponse
      {
        ResultStatus = RequestErrorStatus.OK,
        ProfileCells = cells
      };

      var result = SimpleBinarizableInstanceTester.TestClass(response, "Custom ProfileRequestResponse not same after round trip serialisation");

      Assert.True(result != null);
    }
  }
}
