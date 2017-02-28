using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiTests.Models
{
  [TestClass]
  public class AssetIdRequestTests
    {
    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase1()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");
      Assert.ThrowsException<ServiceException>(() => assetIdRequest.Validate());
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase2()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "ASerial5");
      Assert.ThrowsException<ServiceException>(() => assetIdRequest.Validate());
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase3()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 100, "ASerial5");
      Assert.ThrowsException<ServiceException>(() => assetIdRequest.Validate());
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
