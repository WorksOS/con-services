using System.Collections.Generic;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Patches
{
  public class ToFromBinary_PatchRequestResponse : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_PatchRequestResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<PatchRequestResponse>("Empty PatchRequestResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_PatchRequestResponse()
    {
      var response = new PatchRequestResponse()
      {
        ResultStatus = RequestErrorStatus.OK,
        TotalNumberOfPagesToCoverFilteredData = 1000,
        SubGrids = new List<IClientLeafSubGrid>()
      };

      var cmvSubGrid = new ClientCMVLeafSubGrid();
      cmvSubGrid.FillWithTestPattern();
      response.SubGrids.Add(cmvSubGrid);

      var passCountSubGrid = new ClientPassCountLeafSubGrid();
      passCountSubGrid.FillWithTestPattern();
      response.SubGrids.Add(passCountSubGrid);

      var mdpSubGrid = new ClientMDPLeafSubGrid();
      mdpSubGrid.FillWithTestPattern();
      response.SubGrids.Add(mdpSubGrid);


      SimpleBinarizableInstanceTester.TestClass(response, "Custom PatchRequestResponse not same after round trip serialisation");
    }
  }
}
