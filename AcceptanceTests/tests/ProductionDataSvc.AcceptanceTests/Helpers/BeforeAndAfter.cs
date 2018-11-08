using System.Net.Http;
using ProductionDataSvc.AcceptanceTests.Utils;

namespace ProductionDataSvc.AcceptanceTests.Helpers
{
  public static class BeforeAndAfter
  {
   // [BeforeScenario("requireSurveyedSurfaceLargerThanProductionData")]
    public static void CreateSurveyedSurfaceLargerThanProductionData()
    {
      RestClient.SendHttpClientRequest(
        RestClient.ProdSvcBaseUri,
        "/api/v1/surveyedsurfaces",
        HttpMethod.Post,
        MediaTypes.JSON,
        MediaTypes.JSON,
          @"{
                      ""ProjectId"": 1001158,
                      ""SurveyedSurface"": {
                        ""id"": 111,
                        ""file"": {
                          ""filespaceId"": ""u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01"",
                          ""path"": ""/SurveyedSurfaceAcceptanceTests/1001158"",
                          ""fileName"": ""Original Ground Survey - Dimensions 2012.ttm""
                        },
                        ""offset"": 0.0
                      },
                      ""SurveyedUtc"": ""2015-03-15T18:13:09.265Z""
                }").ConfigureAwait(false);
    }

   // [BeforeScenario("requireSurveyedSurface")]
    public static void CreateSurveyedSurface()
    {
      RestClient.SendHttpClientRequest(
        RestClient.ProdSvcBaseUri,
        "/api/v1/surveyedsurfaces",
        HttpMethod.Post,
        MediaTypes.JSON,
        MediaTypes.JSON,
          @"{
                    ""ProjectId"": 1001158,
                    ""SurveyedSurface"": {
                    ""id"": 111,
                    ""file"": {
                        ""filespaceId"": ""u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01"",
                        ""path"": ""/SurveyedSurfaceAcceptanceTests/1001158"",
                        ""fileName"": ""Milling - Milling.ttm""
                    },
                    ""offset"": 0.0
                    },
                    ""SurveyedUtc"": ""2015-03-15T18:13:09.265Z""
            }").ConfigureAwait(false);
    }

   // [BeforeScenario("requireOldSurveyedSurface")]
    public static void CreateOldSurveyedSurface()
    {
      RestClient.SendHttpClientRequest(
        RestClient.ProdSvcBaseUri,
        "/api/v1/surveyedsurfaces",
        HttpMethod.Post,
        MediaTypes.JSON,
        MediaTypes.JSON,
          @"{
                    ""ProjectId"": 1001158,
                    ""SurveyedSurface"": {
                    ""id"": 111,
                    ""file"": {
                        ""filespaceId"": ""u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01"",
                        ""path"": ""/SurveyedSurfaceAcceptanceTests/1001158"",
                        ""fileName"": ""Milling - Milling.ttm""
                    },
                    ""offset"": 0.0
                    },
                    ""SurveyedUtc"": ""2010-03-15T18:13:09.265Z""
            }").ConfigureAwait(false);
    }

  // [AfterScenario("requireSurveyedSurface")]
  // [AfterScenario("requireOldSurveyedSurface")]
  // [AfterScenario("requireSurveyedSurfaceLargerThanProductionData")]
    private static void DeleteSurveyedSurfaceFile()
    {
      RestClient.SendHttpClientRequest(
        RestClient.ProdSvcBaseUri,
        "/api/v1/projects/1001158/surveyedsurfaces/111/delete",
        HttpMethod.Get,
        MediaTypes.JSON,
        MediaTypes.JSON,
        null).ConfigureAwait(false);
    }
  }
}
