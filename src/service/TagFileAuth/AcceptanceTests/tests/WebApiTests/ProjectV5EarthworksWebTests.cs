using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using Xunit;

namespace WebApiTests
{
  public class ProjectV5EarthworksWebTests : ExecutorTestData
  {
    [Fact]
    public async Task NoProjectProvided_Happy_DeviceAndSingleProjectFound()
    {
      var platformSerial = dimensionsSerial;
      double latitude = 15;
      double longitude = 180;

      var request = new GetProjectUidsBaseRequest(platformSerial, latitude, longitude);
      request.Validate();
      var expectedResult = new GetProjectUidsResult(dimensionsProjectUid, dimensionsSerialDeviceUid, dimensionsCustomerUID, 0, "success");

      var result = await tagFileAuthProjectV5Proxy.GetProjectUidsEarthWorks(request);

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

