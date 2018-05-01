using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests.Models
{
  [TestClass]
  public class AssetIdRequestTests : ModelBaseTests
  {
    private string assetIdPrefix = @"{{""assetId"":-1,""machineLevel"":0,";

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase1()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");
      var ex = Assert.ThrowsException<ServiceException>(() => assetIdRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(24);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(assetIdPrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase2()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(100, 0, "ASerial5");
      assetIdRequest.Validate();
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase3()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 100, "ASerial5");
      var ex = Assert.ThrowsException<ServiceException>(() => assetIdRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(25);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(assetIdPrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
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
