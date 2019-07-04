using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.ResultsHandling;

namespace VSS.Hydrology.Tests.Hydro.Models
{
  [TestClass]
  public class HydroRequestTests
  {
   
    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000", "44abf851-44c5-e311-aa77-00505688274d", "resultantFileName.zip", 1.0, 2001, "Invalid ProjectUid.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "00000000-0000-0000-0000-000000000000", "resultantFileName.zip", 1.0, 2002, "Invalid FilterUid.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "resultantFileName.png", 0.005, 2003, "Must have a valid resultant zip file name.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "", 0.004, 2003, "Must have a valid resultant zip file name.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "resultantFileName.zip", 0.004, 2004, "Resolution must be between 0.005 and < 1,000,000.")]
    public void ValidateRequest(string projectUid, string filterUid, string fileName, double resolution, int expectedErrorCode, string expectedErrorMessage)
    {
      var options = new HydroOptions(resolution);
      var request =
        new HydroRequest(Guid.Parse(projectUid), Guid.Parse(filterUid), options, fileName);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(expectedErrorCode, ex.GetResult.Code);
      Assert.AreEqual(expectedErrorMessage, ex.GetResult.Message);
    }

    [TestMethod]
     [DataRow("33abf851-44c5-e311-aa77-00505688274d", "resultantFileName.zip", 1.0)]
    public void ValidateRequestFilter(string projectUid, string fileName, double resolution)
    {
      var options = new HydroOptions(resolution);
      var request =
        new HydroRequest(Guid.Parse(projectUid), null, options, fileName);

      request.Validate();
    }
  }
}
