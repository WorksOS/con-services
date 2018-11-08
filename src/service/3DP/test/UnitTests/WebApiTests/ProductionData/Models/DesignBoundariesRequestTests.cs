using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models
{
  [TestClass]
  public class DesignBoundariesRequestTests
  {
    [TestMethod]
    public void CanCreateDesignBoundariesRequestTest()
    {
      var validator = new DataAnnotationsValidator();
      DesignBoundariesRequest request = DesignBoundariesRequest.CreateDesignBoundariesRequest(projectId, tolerance);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(request, out results));

      // Missing project id
      request = DesignBoundariesRequest.CreateDesignBoundariesRequest(-1, tolerance);
      Assert.IsFalse(validator.TryValidate(request, out results));
    }

    private long projectId = 1234;
    private double tolerance = 0.05;
  }
}
