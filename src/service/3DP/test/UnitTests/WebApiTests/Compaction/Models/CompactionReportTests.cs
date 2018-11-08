using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApiTests.Compaction.Models
{
  [TestClass]
  public class CompactionReportTests
  {
    [TestMethod]
    public void CanCreateCompactionReportGridRequestTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      // GridReportOption - Automatic
      CompactionReportGridRequest request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        1, null, -1, null, true, true, true, true, true, false, null, 1.0, GridReportOption.Automatic, 0.0, 0.0, 0.0, 0.0, 0.0);
      Assert.IsTrue(validator.TryValidate(request, out results));

      // GridReportOption - Manual (Second/End Point)
      request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        1, null, -1, null, true, true, true, true, true, false, null, 1.0, GridReportOption.EndPoint, 1230.0, 2827.0,12350.0, 28340.0, 0.0);
      Assert.IsTrue(validator.TryValidate(request, out results));

      // GridReportOption - Manual (Direction/Azimuth)
      request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        1, null, -1, null, true, true, true, true, true, false, null, 1.0, GridReportOption.Direction, 0.0, 0.0, 0.0, 0.0, Math.PI);
      Assert.IsTrue(validator.TryValidate(request, out results));
    }

    [TestMethod]
    public void ValidateCompactionReportGridRequestTest()
    {
      // GridReportOption - Automatic
      CompactionReportGridRequest request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        1, null, -1, null, true, true, true, true, true, false, null, 1.0, GridReportOption.Automatic, 0.0, 0.0, 0.0, 0.0, 0.0);
      request.Validate();

      // GridReportOption - Manual (Second/End Point)
      request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        1, null, -1, null, true, true, true, true, true, false, null, 1.0, GridReportOption.EndPoint, 1230.0, 2827.0, 12350.0, 28340.0, 0.0);
      request.Validate();

      // GridReportOption - Manual (Direction/Azimuth)
      request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        1, null, -1, null, true, true, true, true, true, false, null, 1.0, GridReportOption.Direction, 0.0, 0.0, 0.0, 0.0, Math.PI);
      request.Validate();
    }
  }
}
