using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApiTests.Compaction.Models
{
  [TestClass]
  public class LineworkRequestTests
  {
    [TestMethod]
    [DataRow(23, 23)]
    [DataRow(-1, VelociraptorConstants.MAX_BOUNDARIES_TO_PROCESS)]
    [DataRow(0, VelociraptorConstants.MAX_BOUNDARIES_TO_PROCESS)]
    public void Should_return_correct_boundary_for_request(int maxBoundariesToProcess, int expectedResult)
    {
      var request = new LineworkRequest(new DxfFileRequest
      {
        MaxBoundariesToProcess = maxBoundariesToProcess
      }, null);

      Assert.AreEqual(expectedResult, request.NumberOfBoundariesToProcess);
    }
  }
}
