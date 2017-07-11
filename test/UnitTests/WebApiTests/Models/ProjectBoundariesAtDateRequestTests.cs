using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Productivity3D.WebApiModels.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectBoundariesAtDateRequestTests
  {
    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase1()
    {
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(-1, DateTime.UtcNow);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundariesAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(String.Format("Must have assetId", -1), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase2()
    {
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(-1, DateTime.UtcNow.AddYears(-1));
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundariesAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(String.Format("Must have assetId", -1), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase3()
    {
      DateTime now = DateTime.UtcNow.AddYears(-50).AddMonths(-1);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(1233, now);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundariesAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("tagFileUTC must have occured within last 50 years", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase4()
    {
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(23434, DateTime.UtcNow.AddYears(-1));
      projectBoundariesAtDateRequest.Validate();
    }

  }
}
