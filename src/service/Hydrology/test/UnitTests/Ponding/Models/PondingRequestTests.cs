using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.ResultsHandling;

namespace VSS.Hydrology.Tests.Ponding.Models
{
  [TestClass]
  public class PondingRequestTests
  {
    protected HydroErrorCodesProvider _hydroErrorCodesProvider = new HydroErrorCodesProvider();


    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000", "44abf851-44c5-e311-aa77-00505688274d", 1.0, true, "theFileName.png", 2001)]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", 1.0, true, "theFileName.png", 2002)]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "00000000-0000-0000-0000-000000000000", 0.004, true, "theFileName.png", 2003)]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "00000000-0000-0000-0000-000000000000", 1.0, true, "", 2004)]
    public void ValidateRequest(string projectUid, string filterUid, double resolution, bool isMetric, string fileName, int expectedErrorCode)
    {
      var request =
        new PondingRequest(Guid.Parse(projectUid), Guid.Parse(filterUid), resolution, isMetric, fileName);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(expectedErrorCode, ex.GetResult.Code);
    }
  }
}
