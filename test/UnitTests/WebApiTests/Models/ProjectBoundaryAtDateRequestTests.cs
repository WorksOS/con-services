using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiTests.Models
{
  [TestClass]
  public class ProjectBoundaryAtDateRequestTests
  {
    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase1()
    {
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(-1, DateTime.UtcNow);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundaryAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(String.Format("Must have projectID {0}", -1)));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase2()
    {
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(-1, DateTime.UtcNow.AddYears(-1));
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundaryAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(String.Format("Must have projectID {0}", -1)));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase3()
    {
      DateTime now = DateTime.UtcNow.AddYears(-5).AddMonths(-1);
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(1233, now);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundaryAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(string.Format("tagFileUTC must have occured within last 5 years {0}", now)));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase4()
    {
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(23434, DateTime.UtcNow.AddYears(-1));
      projectBoundaryAtDateRequest.Validate();
    }

  }
}
