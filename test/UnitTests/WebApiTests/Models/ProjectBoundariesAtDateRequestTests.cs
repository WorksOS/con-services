using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiTests.Models
{
  [TestClass]
  public class ProjectBoundariesAtDateRequestTests
  {
    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase1()
    {
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(-1, DateTime.UtcNow);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundariesAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(String.Format("Must have assetId {0}", -1)));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase2()
    {
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(-1, DateTime.UtcNow.AddYears(-1));
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundariesAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(String.Format("Must have assetId {0}", -1)));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase3()
    {
      DateTime now = DateTime.UtcNow.AddYears(-5).AddMonths(-1);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(1233, now);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundariesAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(string.Format("tagFileUTC must have occured within last 5 years {0}", now)));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase4()
    {
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(23434, DateTime.UtcNow.AddYears(-1));
      projectBoundariesAtDateRequest.Validate();
    }

  }
}
