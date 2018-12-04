using Microsoft.VisualStudio.TestTools.UnitTesting;
using VLPDDecls;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApiTests.Compaction.Models
{
  [TestClass]
  public class LineworkRequestTests
  {
    [TestMethod]
    [DataRow(null, 1)]
    [DataRow(" ", 1)]
    [DataRow("filename", VLPDDecls.__Global.MAX_BOUNDARIES_TO_PROCESS)]
    public void Should_return_correct_boundary_for_request(string filename, int expectedResult)
    {
      var request = LineworkRequest.Create(1234, null, TVLPDDistanceUnits.vduMeters, filename);

      Assert.AreEqual(expectedResult, request.NumberOfBoundariesToProcess);
    }
  }
}
