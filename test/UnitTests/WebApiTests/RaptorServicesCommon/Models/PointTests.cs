
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;


namespace VSS.Raptor.Service.WebApiTests.Common.Models
{
  [TestClass()]
  public class PointTests
  {
    [TestMethod()]
    public void CanCreatePointTest()
    {
      var validator = new DataAnnotationsValidator();
      Point point = Point.CreatePoint(35.0, 72.5);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(point, out results));
    }
  }
}
