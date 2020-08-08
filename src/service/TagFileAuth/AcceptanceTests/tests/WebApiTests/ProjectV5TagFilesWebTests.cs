using System;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using Xunit;
using Assert = Xunit.Assert;

namespace WebApiTests
{
  [Collection("Service collection")]
  public class ProjectV5TagFilesWebTests 
  {
    public ProjectV5TagFilesWebTests()
    { }

    [Fact]
    public async Task NoProjectProvided_Auto_Happy_DeviceAndSingleProjectFound()
    {
      var platformSerial = ExecutorTestFixture.dimensionsSerial;
      var latitude = 15.0;
      var longitude = 180.0;

      var request = new GetProjectUidsRequest(string.Empty, platformSerial, latitude, longitude);
      request.Validate();
      var expectedResult = new GetProjectUidsResult(ExecutorTestFixture.dimensionsProjectUid, ExecutorTestFixture.dimensionsSerialDeviceUid, ExecutorTestFixture.dimensionsCustomerUID, 0, "success");

      var result = await ExecutorTestFixture.tagFileAuthProjectV5Proxy.GetProjectUids(request);

      ValidateResult(result, expectedResult);
    }

    [Fact(Skip ="Aaron to re enable when moved into integration test.")]
    public async Task NoProjectProvided_Auto_Happy_DeviceAndSingleProjectFound_UsingNE()
    {
      var platformSerial = ExecutorTestFixture.dimensionsSerial;
      var northing = 2300.77;
      var easting = 1650.66;

      var request = new GetProjectUidsRequest(string.Empty, platformSerial, 0, 0, northing, easting);
      request.Validate();
      var expectedResult = new GetProjectUidsResult(ExecutorTestFixture.dimensionsProjectUid, ExecutorTestFixture.dimensionsSerialDeviceUid, ExecutorTestFixture.dimensionsCustomerUID, 0, "success");

      var result = await ExecutorTestFixture.tagFileAuthProjectV5Proxy.GetProjectUids(request);

      ValidateResult(result, expectedResult);
    }

    [Fact]
    public async Task ProjectProvided_Manual_Sad_ProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var platformSerial = ExecutorTestFixture.dimensionsSerial;
      var latitude = 89.0;
      var longitude = 130.0;

      var request = new GetProjectUidsRequest(projectUid, platformSerial, latitude, longitude);
      request.Validate();
      var expectedResult = new GetProjectUidsResult(string.Empty, ExecutorTestFixture.dimensionsSerialDeviceUid, ExecutorTestFixture.dimensionsCustomerUID, 3038, "Manual Import: Unable to find the Project requested");

      var result = await ExecutorTestFixture.tagFileAuthProjectV5Proxy.GetProjectUids(request);

      ValidateResult(result, expectedResult);
    }

    [Fact]
    public async Task NoProjectProvided_Auto_Sad_DeviceNotFound()
    {
      var platformSerial = Guid.NewGuid().ToString();
      var latitude = 89.0;
      var longitude = 130.0;

      var request = new GetProjectUidsRequest(string.Empty, platformSerial, latitude, longitude);
      request.Validate();
      var expectedResult = new GetProjectUidsResult(string.Empty, string.Empty, string.Empty, uniqueCode: 3100, "Unable to locate device by serialNumber in cws");

      var result = await ExecutorTestFixture.tagFileAuthProjectV5Proxy.GetProjectUids(request);

      ValidateResult(result, expectedResult);
    }

    private void ValidateResult(GetProjectUidsResult actualResult, GetProjectUidsResult expectedResult)
    {
      Assert.NotNull(actualResult);
      Assert.Equal(expectedResult.ProjectUid, actualResult.ProjectUid);
      Assert.Equal(expectedResult.DeviceUid, actualResult.DeviceUid);
      Assert.Equal(expectedResult.CustomerUid, actualResult.CustomerUid);
      Assert.Equal(expectedResult.Code, actualResult.Code);
      Assert.Equal(expectedResult.Message, actualResult.Message);
    }
  }
}

