using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models
{
  [TestClass]
  public class CellPassesRequestTest
  {
    [TestMethod]
    public void CPR_CanCreateCellPassesRequestTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      CellPassesRequest cpRequest = CellPassesRequest.CreateCellPassRequest(544, null, null, null, null, 0, 0,
          null);
      Assert.IsTrue(validator.TryValidate(cpRequest, out results));
      
      // invalid projectid
      cpRequest = CellPassesRequest.CreateCellPassRequest(-1, null, null, null, null, 0, 0,null);
      Assert.IsFalse(validator.TryValidate(cpRequest, out results));

      // full data
      CellAddress cellAddress = CellAddress.CreateCellAddress(1, 2);
      Point point = Point.CreatePoint(1.0, 2.0);
      WGSPoint wgsPoint = WGSPoint.CreatePoint(1.0, 2.0);
      LiftBuildSettings settings = LiftBuildSettings.CreateLiftBuildSettings(
        CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
        LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
        false, 0.0f, 0, 0, null, null, null, LiftThicknessTarget.HelpSample, null);
      Filter filter = Filter.CreateFilter(null, null, null, null, null, 1, new List<long>(), true, false, null,
          new List<WGSPoint>(),
          new List<Point>(),
          false,
          DesignDescriptor.CreateDesignDescriptor(1,FileDescriptor.EmptyFileDescriptor,0),
          0, 0, 0, 0,
          "", null,
          DesignDescriptor.CreateDesignDescriptor(1, FileDescriptor.EmptyFileDescriptor, 0),
          0, 0,0,
          new List<MachineDetails>(),
          new List<long>(),
          false, GPSAccuracy.Medium, false, null, null, null);

      cpRequest = CellPassesRequest.CreateCellPassRequest(544, cellAddress, point, wgsPoint, settings,
                                                          0, 0, filter);
      Assert.IsTrue(validator.TryValidate(cpRequest, out results));
    }

    [TestMethod]
    public void CPR_CellPassesRequestValidateTest()
    {
      // test that all three cell address types are not set
      CellPassesRequest cpRequest = CellPassesRequest.CreateCellPassRequest(544, null, null, null, null, 0, 0,null);
      Assert.ThrowsException<ServiceException>(() => cpRequest.Validate());

    }


  }
}
