using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Hydrology.WebApi.Abstractions.Models;

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
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "resultantFileName.zip", 0.004, 2004, "Resolution must be between 0.5 and 20.")]
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
     [DataRow("33abf851-44c5-e311-aa77-00505688274d", "resultantFileName.zip")]
    public void ValidateRequestFilter(string projectUid, string fileName)
    {
      var options = new HydroOptions();
      var request =
        new HydroRequest(Guid.Parse(projectUid), null, options, fileName);

      request.Validate();
      Assert.AreEqual(1, request.Options.Resolution);
      Assert.AreEqual(10, request.Options.Levels);
    }

    [TestMethod]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "resultantFileName.zip", 0.6, 1, 2008, "Levels must be between 2 and 20.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "resultantFileName.zip", 0.6, 241, 2008, "Levels must be between 2 and 20.")]
    public void ValidateRequestPonding(string projectUid, string filterUid, string fileName, double resolution, int levels, int expectedErrorCode, string expectedErrorMessage)
    {
      var options = new HydroOptions(resolution, levels);
      var request =
        new HydroRequest(Guid.Parse(projectUid), Guid.Parse(filterUid), options, fileName);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(expectedErrorCode, ex.GetResult.Code);
      Assert.AreEqual(expectedErrorMessage, ex.GetResult.Message);
    }

    [TestMethod]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "res.zip", 0.0001, 1000, "", "", "", "", 2018, "MinSlope must be between 0.005 and 99.0.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "res.zip", 0.1, 1000, "", "", "", "", 2019, "MaxSlope must be between 0.006 and 100.0.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "res.zip", 1.0, 0.2, "", "", "", "", 2020, "MaxSlope must be greater than MinSlope.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "res.zip", 0.1, 0.2, "bb", "", "", "", 2021, "VortexViolationColor must be a valid color.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "res.zip", 0.1, 0.2, "Khaki", "", "", "", 2021, "MaxSlopeViolationColor must be a valid color.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "res.zip", 0.1, 0.2, "Khaki", "IndianRed", "aa", "", 2021, "MinSlopeViolationColor must be a valid color.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "res.zip", 0.1, 0.2, "Khaki", "IndianRed", "LightGreen", "", 2021, "NoViolationColorDark must be a valid color.")]
    [DataRow("33abf851-44c5-e311-aa77-00505688274d", "44abf851-44c5-e311-aa77-00505688274d", "res.zip", 0.1, 0.2, "Khaki", "IndianRed", "LightGreen", "", 2021, "NoViolationColorDark must be a valid color.")]
    public void ValidateRequestDrainageViolation(string projectUid, string filterUid, string fileName, 
      double minSlope, double maxSlope,
      string vortexViolationColor, string maxSlopeViolationColor,
      string minSlopeViolationColor, string noViolationColor,
      int expectedErrorCode, string expectedErrorMessage)
    {
      var options = new HydroOptions(minSlope: minSlope, maxSlope: maxSlope,
        vortexViolationColor: vortexViolationColor, maxSlopeViolationColor: maxSlopeViolationColor,
        minSlopeViolationColor: minSlopeViolationColor, noViolationColor: noViolationColor
       );
      var request =
        new HydroRequest(Guid.Parse(projectUid), Guid.Parse(filterUid), options, fileName);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(expectedErrorCode, ex.GetResult.Code);
      Assert.AreEqual(expectedErrorMessage, ex.GetResult.Message);
    }

  }
}
