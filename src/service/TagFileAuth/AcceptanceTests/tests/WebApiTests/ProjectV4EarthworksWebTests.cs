using System;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using Xunit;

namespace WebApiTests
{
  public class ProjectV4EarthworksWebTests : ExecutorTestData
  {
    [Fact(Skip = "until mockProjectWebApi deployed")]
    public async Task Auto_Happy()
    {
      // this test can be made to work through TFA service, through to ProjectSvc - if you setup environment variables appropriately
      var CBRadioserial = dimensionsSerial;
      var EC50Serial = string.Empty;
      double latitude = 89;
      double longitude = 130;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsEarthWorksRequest = new GetProjectAndAssetUidsEarthWorksRequest(CBRadioserial,
        EC50Serial, latitude, longitude, tagFileTimestamp);
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

