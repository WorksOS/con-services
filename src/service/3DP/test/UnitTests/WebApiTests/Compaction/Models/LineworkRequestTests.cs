using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApiTests.Compaction.Models
{
  [TestClass]
  public class LineworkRequestTests
  {
    [TestMethod]
    [DataRow(null, null, 1)]
    [DataRow(" ", null, 1)]
    [DataRow("filename", "csName", VLPDDecls.__Global.MAX_BOUNDARIES_TO_PROCESS)]
    public void Should_return_correct_boundary_for_request(string filename, string coordinateSystemName, int expectedResult)
    {
      var request = LineworkRequest.Create(new DxfFileRequest
      {
        Filename = filename,
        CoordinateSystemName = coordinateSystemName
      }, null);

      Assert.AreEqual(expectedResult, request.NumberOfBoundariesToProcess);
    }
  }
}
