using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WebApiModels.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectBoundaryAtDateRequestTests
  {
    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase1()
    {
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(-1, DateTime.UtcNow);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundaryAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have projectId", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase2()
    {
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(-1, DateTime.UtcNow.AddYears(-1));
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundaryAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have projectId", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase3()
    {
      DateTime now = DateTime.UtcNow.AddYears(-50).AddMonths(-1);
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(1233, now);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundaryAtDateRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("tagFileUTC must have occured within last 50 years", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase4()
    {
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(23434, DateTime.UtcNow.AddYears(-1));
      projectBoundaryAtDateRequest.Validate();
    }

  }
}
