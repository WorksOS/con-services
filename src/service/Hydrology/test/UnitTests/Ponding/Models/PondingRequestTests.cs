using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSS.Hydrology.Tests.Ponding.Models
{
  [TestClass]
  public class PondingRequestTests
  {
    [TestMethod]
    [DataRow(23, 23)]
    public void ValidateRequest(int maxBoundariesToProcess, int expectedResult)
    {
      //var request = new LineworkRequest(new DxfFileRequest
      //{
      //  MaxBoundariesToProcess = maxBoundariesToProcess
      //}, null);

      //Assert.AreEqual(expectedResult, request.NumberOfBoundariesToProcess);
      Assert.Fail();
    }
  }
}
