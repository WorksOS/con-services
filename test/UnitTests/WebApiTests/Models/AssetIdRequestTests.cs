using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiModels.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class AssetIdRequestTests
    {
    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase1()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");
      var ex = Assert.ThrowsException<ServiceException>(() => assetIdRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have assetId and/or projectID", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase2()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "ASerial5");
      var ex = Assert.ThrowsException<ServiceException>(() => assetIdRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("AssetId must have valid deviceType", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase3()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 100, "ASerial5");
      var ex = Assert.ThrowsException<ServiceException>(() => assetIdRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("AssetId must have valid deviceType", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase4()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(456661, 0, "");
      assetIdRequest.Validate();
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase5()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 6, "AGo ial6");
      assetIdRequest.Validate();
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase6()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(745651, 6, "AGo ial6");
      assetIdRequest.Validate();
    }

  }
}
