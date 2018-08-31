using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using WGSPoint = VSS.Productivity3D.Models.Models.WGSPoint3D;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models
{
  [TestClass]
  public class CellPassesRequestTests
  {
    [TestMethod]
    public void CPR_CanCreateCellPassesRequestTest()
    {
      var liftThicknessTarget = new LiftThicknessTarget{
        AboveToleranceLiftThickness = (float)0.001,
        BelowToleranceLiftThickness = (float)0.002,
        TargetLiftThickness = (float)0.05};

      var validator = new DataAnnotationsValidator();

      CellPassesRequest cpRequest = CellPassesRequest.CreateCellPassRequest(544, null, null, null, null, 0, 0,
          null);
      Assert.IsTrue(validator.TryValidate(cpRequest, out ICollection<ValidationResult> results));

      // invalid projectid
      cpRequest = CellPassesRequest.CreateCellPassRequest(-1, null, null, null, null, 0, 0, null);
      Assert.IsFalse(validator.TryValidate(cpRequest, out results));

      // full data
      CellAddress cellAddress = CellAddress.CreateCellAddress(1, 2);
      Point point = Point.CreatePoint(1.0, 2.0);
      WGSPoint wgsPoint = new WGSPoint3D(1.0, 2.0);
      LiftBuildSettings settings = new LiftBuildSettings(
        new CCVRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
        LiftThicknessType.Compacted, new MDPRangePercentage(35.0, 75.0),
        false, 0.0f, 0, 0, null, null, null, liftThicknessTarget, null);

      var filter = FilterResult.CreateFilter(null, null, null, null, null, 1, new List<long>(), true, false, null,
          new List<WGSPoint>(),
          new List<Point>(),
          false,
          new DesignDescriptor(1, FileDescriptor.EmptyFileDescriptor, 0),
          0, 0, 0, 0,
          "", null,
          new DesignDescriptor(1, FileDescriptor.EmptyFileDescriptor, 0),
          0, 0, 0,
          new List<MachineDetails>(),
          new List<long>(),
          false, GPSAccuracy.Medium, false, null, null, null,
          new DesignDescriptor(1, FileDescriptor.EmptyFileDescriptor, 0), null, null, null, null, null);

      cpRequest = CellPassesRequest.CreateCellPassRequest(544, cellAddress, point, wgsPoint, settings,
                                                          0, 0, filter);
      Assert.IsTrue(validator.TryValidate(cpRequest, out results));
    }

    [TestMethod]
    public void CPR_CellPassesRequestValidateTest()
    {
      // test that all three cell address types are not set
      CellPassesRequest cpRequest = CellPassesRequest.CreateCellPassRequest(544, null, null, null, null, 0, 0, null);
      Assert.ThrowsException<ServiceException>(() => cpRequest.Validate());
    }
  }
}
