using System;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using Xunit;
using Assert = Xunit.Assert;

namespace WebApiTests
{
  public class ProjectV5TagFilesWebTests : ExecutorTestData
  {
    public ProjectV5TagFilesWebTests()
    { }

    [Fact]
    public async Task NoProjectProvided_Auto_Happy_DeviceAndSingleProjectFound()
    {
      var platformSerial = dimensionsSerial;
      var latitude = 15.0;
      var longitude = 180.0;

      var request = new GetProjectUidsRequest(string.Empty, platformSerial, latitude, longitude);
      request.Validate();
      var expectedResult = new GetProjectAndAssetUidsResult(dimensionsProjectUid, dimensionsSerialDeviceUid, dimensionsCustomerUID, 0, "success");

      var result = await tagFileAuthProjectV5Proxy.GetProjectUids(request);

      ValidateResult(result, expectedResult);
    }

    [Fact]
    public async Task NoProjectProvided_Auto_Happy_DeviceAndSingleProjectFound_UsingNE()
    {
      var platformSerial = dimensionsSerial;
      var northing = 2300.77;
      var easting = 1650.66;

      var request = new GetProjectUidsRequest(string.Empty, platformSerial, 0, 0, northing, easting);
      request.Validate();
      var expectedResult = new GetProjectAndAssetUidsResult(dimensionsProjectUid, dimensionsSerialDeviceUid, dimensionsCustomerUID, 0, "success");

      var result = await tagFileAuthProjectV5Proxy.GetProjectUids(request);

      ValidateResult(result, expectedResult);
    }

    [Fact]
    public async Task ProjectProvided_Manual_Sad_ProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var platformSerial = dimensionsSerial;
      var latitude = 89.0;
      var longitude = 130.0;

      var request = new GetProjectUidsRequest(projectUid, platformSerial, latitude, longitude);
      request.Validate();
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty, string.Empty, 3038, "Manual Import: Unable to find the Project requested");

      var result = await tagFileAuthProjectV5Proxy.GetProjectUids(request);

      ValidateResult(result, expectedGetProjectAndAssetUidsResult);
    }

    [Fact]
    public async Task NoProjectProvided_Auto_Sad_DeviceNotFound()
    {
      var platformSerial = Guid.NewGuid().ToString();
      var latitude = 89.0;
      var longitude = 130.0;

      var request = new GetProjectUidsRequest(string.Empty, platformSerial, latitude, longitude);
      request.Validate();
      var expectedResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty, string.Empty, uniqueCode: 3100, "Unable to locate device by serialNumber in cws");

      var result = await tagFileAuthProjectV5Proxy.GetProjectUids(request);

      ValidateResult(result, expectedResult);
    }

    private void ValidateResult(GetProjectAndAssetUidsResult actualResult, GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult)
    {
      Assert.NotNull(actualResult);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.ProjectUid, actualResult.ProjectUid);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.AssetUid, actualResult.AssetUid);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.CustomerUid, actualResult.CustomerUid);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.Code, actualResult.Code);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.Message, actualResult.Message);
    }
  }
}

