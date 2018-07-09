using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Models
{
  [TestClass]
  public class PointTests
  {
    [TestMethod]
    public void CanCreatePointTest()
    {
      var validator = new DataAnnotationsValidator();
      Point point = Point.CreatePoint(35.0, 72.5);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(point, out results));
    }
  }
}
