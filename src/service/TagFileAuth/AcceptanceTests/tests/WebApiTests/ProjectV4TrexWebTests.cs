using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using Xunit;
using Assert = Xunit.Assert;

namespace WebApiTests
{
  public class ProjectV4TrexWebTests : ExecutorTestData
  {
    public ProjectV4TrexWebTests()
    { }

    [Fact]
    public async System.Threading.Tasks.Task Manual_Sad_ProjectNotFound()
    {
      // this test can be made to work through TFA service, through to ProjectSvc - if you setup environment variables appropriately
      var projectUid = Guid.NewGuid().ToString();
      var CBRadioType = TagFileDeviceTypeEnum.SNM940;
      var CBRadioserial = dimensionsSerial;
      var EC50Serial = string.Empty;
      double latitude = 89;
      double longitude = 130;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)CBRadioType, CBRadioserial,
        EC50Serial, latitude, longitude, tagFileTimestamp);
      getProjectAndAssetUidsRequest.Validate();
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);

      var result = await tagFileAuthProjectProxy.GetProjectAndAssetUids(getProjectAndAssetUidsRequest);

      ValidateResult(result, expectedGetProjectAndAssetUidsResult, 3038, "Unable to find the Project requested");
    }

    [Fact]
    [Ignore] // todoJeannie
    public async System.Threading.Tasks.Task Auto_Sad_DeviceNotFound()
    {
      // this test can be made to work through TFA service, through to ProjectSvc - if you setup environment variables appropriately
      var CBRadioType = TagFileDeviceTypeEnum.SNM940;
      var CBRadioserial = Guid.NewGuid().ToString();
      var EC50Serial = string.Empty;
      double latitude = 89;
      double longitude = 130;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)CBRadioType, CBRadioserial,
        EC50Serial, latitude, longitude, tagFileTimestamp);
      getProjectAndAssetUidsRequest.Validate();
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);

      var result = await tagFileAuthProjectProxy.GetProjectAndAssetUids(getProjectAndAssetUidsRequest);

      ValidateResult(result, expectedGetProjectAndAssetUidsResult, 3100, "Unable to locate device by serialNumber in cws");
    }

    private void ValidateResult(GetProjectAndAssetUidsResult actualResult, GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult,
      int resultCode, string resultMessage)
    {
      Assert.NotNull(actualResult);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.ProjectUid, actualResult.ProjectUid);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.DeviceUid, actualResult.DeviceUid);
      Assert.Equal(resultCode, actualResult.Code);
      Assert.Equal(resultMessage, actualResult.Message);
    }
  }
}

