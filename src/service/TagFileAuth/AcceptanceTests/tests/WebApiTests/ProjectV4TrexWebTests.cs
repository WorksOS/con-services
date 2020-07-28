using System;
using System.Threading.Tasks;
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
    public async Task NoProjectProvided_Auto_Happy_DeviceAndSingleProjectFound()
    {
      var cbRadioType = TagFileDeviceTypeEnum.SNM940;
      var cbRadioSerial = dimensionsSerial;
      var ec50Serial = string.Empty;
      var latitude = 15.0;
      var longitude = 180.0;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)cbRadioType, cbRadioSerial,
        ec50Serial, latitude, longitude, tagFileTimestamp);
      getProjectAndAssetUidsRequest.Validate();
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(dimensionsProjectUid, dimensionsSerialDeviceUid, 0, "success");

      var result = await tagFileAuthProjectProxy.GetProjectAndAssetUids(getProjectAndAssetUidsRequest);

      ValidateResult(result, expectedGetProjectAndAssetUidsResult);
    }

    [Fact]
    public async Task NoProjectProvided_Auto_Happy_DeviceAndSingleProjectFound_UsingNE()
    {
      var cbRadioType = TagFileDeviceTypeEnum.SNM940;
      var cbRadioSerial = dimensionsSerial;
      var ec50Serial = string.Empty;
      var northing = 2300.77;
      var easting = 1650.66;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)cbRadioType, cbRadioSerial,
        ec50Serial, 0, 0, tagFileTimestamp, northing, easting);
      getProjectAndAssetUidsRequest.Validate();
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(dimensionsProjectUid, dimensionsSerialDeviceUid, 0, "success");

      var result = await tagFileAuthProjectProxy.GetProjectAndAssetUids(getProjectAndAssetUidsRequest);

      ValidateResult(result, expectedGetProjectAndAssetUidsResult);
    }

    [Fact]
    public async Task ProjectProvided_Manual_Sad_ProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var cbRadioType = TagFileDeviceTypeEnum.SNM940;
      var cbRadioSerial = dimensionsSerial;
      var ec50Serial = string.Empty;
      var latitude = 89.0;
      var longitude = 130.0;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)cbRadioType, cbRadioSerial,
        ec50Serial, latitude, longitude, tagFileTimestamp);
      getProjectAndAssetUidsRequest.Validate();
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, dimensionsSerialDeviceUid, 3038, "Manual Import: Unable to find the Project requested");

      var result = await tagFileAuthProjectProxy.GetProjectAndAssetUids(getProjectAndAssetUidsRequest);

      ValidateResult(result, expectedGetProjectAndAssetUidsResult);
    }

    [Fact]
    public async Task NoProjectProvided_Auto_Sad_DeviceNotFound()
    {
      var cbRadioType = TagFileDeviceTypeEnum.SNM940;
      var cbRadioSerial = Guid.NewGuid().ToString();
      var ec50Serial = string.Empty;
      var latitude = 89.0;
      var longitude = 130.0;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)cbRadioType, cbRadioSerial,
        ec50Serial, latitude, longitude, tagFileTimestamp);
      getProjectAndAssetUidsRequest.Validate();
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty, uniqueCode: 3100, "Unable to locate device by serialNumber in cws");

      var result = await tagFileAuthProjectProxy.GetProjectAndAssetUids(getProjectAndAssetUidsRequest);

      ValidateResult(result, expectedGetProjectAndAssetUidsResult);
    }

    private void ValidateResult(GetProjectAndAssetUidsResult actualResult, GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult)
    {
      Assert.NotNull(actualResult);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.ProjectUid, actualResult.ProjectUid);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.DeviceUid, actualResult.DeviceUid);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.Code, actualResult.Code);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.Message, actualResult.Message);
    }
  }
}

