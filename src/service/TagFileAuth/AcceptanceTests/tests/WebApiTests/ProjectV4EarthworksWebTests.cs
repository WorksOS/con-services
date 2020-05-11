using System;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using Xunit;

namespace WebApiTests
{
  public class ProjectV4EarthworksWebTests : ExecutorTestData
  {
    [Fact(Skip = "until mockProjectWebApi deployed")]
    public async Task NoProjectProvided_Happy_DeviceAndSingleProjectFound()
    {
      // this test can be made to work through TFA service, through to ProjectSvc - if you setup environment variables appropriately
      var cbRadioserial = dimensionsSerial;
      var ec50Serial = string.Empty;
      double latitude = 89;
      double longitude = 130;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsEarthWorksRequest = new GetProjectAndAssetUidsEarthWorksRequest(cbRadioserial,
        ec50Serial, latitude, longitude, tagFileTimestamp);
      getProjectAndAssetUidsEarthWorksRequest.Validate();
      var expectedGetProjectAndAssetUidsEarthWorksResult = new GetProjectAndAssetUidsEarthWorksResult(dimensionsProjectUid, dimensionsSerialDeviceUid, dimensionsCustomerUID, true, 0, "success");

      var result = await tagFileAuthProjectProxy.GetProjectAndAssetUidsEarthWorks(getProjectAndAssetUidsEarthWorksRequest);

      ValidateResult(result, expectedGetProjectAndAssetUidsEarthWorksResult);
    }

    private void ValidateResult(GetProjectAndAssetUidsEarthWorksResult actualResult, GetProjectAndAssetUidsEarthWorksResult expectedGetProjectAndAssetUidsEarthWorksResult)
    {
      Assert.NotNull(actualResult);
      Assert.Equal(expectedGetProjectAndAssetUidsEarthWorksResult.ProjectUid, actualResult.ProjectUid);
      Assert.Equal(expectedGetProjectAndAssetUidsEarthWorksResult.AssetUid, actualResult.AssetUid);
      Assert.Equal(expectedGetProjectAndAssetUidsEarthWorksResult.Code, actualResult.Code);
      Assert.Equal(expectedGetProjectAndAssetUidsEarthWorksResult.Message, actualResult.Message);
    }
  }
}

