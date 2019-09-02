using System.Collections.Generic;
using VSS.TRex.Exports.Patches.GridFabric.PatchRequestWithColors;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Patches
{
  public class ToFromBinary_PatchRequestWithColorsResponse : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_PatchRequestWithColorsResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<PatchRequestWithColorsResponse>("Empty PatchRequestWithColorsResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_PatchRequestWithColorsResponse()
    {
      var response = new PatchRequestWithColorsResponse()
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


      SimpleBinarizableInstanceTester.TestClass(response, "Custom PatchRequestWithColorsResponse not same after round trip serialisation");
    }
  }
}
