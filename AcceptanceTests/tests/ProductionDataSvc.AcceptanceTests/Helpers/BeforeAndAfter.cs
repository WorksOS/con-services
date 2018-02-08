using RaptorSvcAcceptTestsCommon.Utils;
using RestAPICoreTestFramework.Utils.Common;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.Helpers
{
  [Binding]
    public class BeforeAndAfter
    {
        [BeforeScenario("requireDummySurveyedSurface")]
        public static void CreateDummySurveyedSurface()
        {
            RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ProdSvcBaseUri + "/api/v1/surveyedsurfaces",
                "POST", RestClientConfig.JsonMediaType,
                @"{
                      ""ProjectId"": 1001158,
                      ""SurveyedSurface"": {
                        ""id"": 111,
                        ""file"": {
                          ""filespaceId"": ""u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01"",
                          ""path"": ""/77561/1158"",
                          ""fileName"": ""Dummy.ttm""
                        },
                        ""offset"": 0.0
                      },
                      ""SurveyedUtc"": ""2015-03-15T18:13:09.265Z""
                }");
        }

        [BeforeScenario("requireSurveyedSurface")]
        public static void CreateSurveyedSurface()
        {
            RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ProdSvcBaseUri + "/api/v1/surveyedsurfaces",
                "POST", RestClientConfig.JsonMediaType,
                @"{
                    ""ProjectId"": 1001158,
                    ""SurveyedSurface"": {
                    ""id"": 111,
                    ""file"": {
                        ""filespaceId"": ""u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01"",
                        ""path"": ""/77561/1158"",
                        ""fileName"": ""Milling - Milling.ttm""
                    },
                    ""offset"": 0.0
                    },
                    ""SurveyedUtc"": ""2015-03-15T18:13:09.265Z""
            }");
        }

        [BeforeScenario("requireOldSurveyedSurface")]
        public static void CreateOldSurveyedSurface()
        {
            RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ProdSvcBaseUri + "/api/v1/surveyedsurfaces",
                "POST", RestClientConfig.JsonMediaType,
                @"{
                    ""ProjectId"": 1001158,
                    ""SurveyedSurface"": {
                    ""id"": 111,
                    ""file"": {
                        ""filespaceId"": ""u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01"",
                        ""path"": ""/77561/1158"",
                        ""fileName"": ""Milling - Milling.ttm""
                    },
                    ""offset"": 0.0
                    },
                    ""SurveyedUtc"": ""2010-03-15T18:13:09.265Z""
            }");
        }

      
        [AfterScenario("requireDummySurveyedSurface")]
        public static void DeleteDummySurveyedSurface()
        {
            RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ProdSvcBaseUri + "/api/v1/projects/1001158/surveyedsurfaces/111/delete",
                "GET", RestClientConfig.JsonMediaType, null);
        }

        [AfterScenario("requireSurveyedSurface")]
        [AfterScenario("requireOldSurveyedSurface")]
        [AfterScenario("requireDummySurveyedSurface")]
        public static void DeleteSurveyedSurface()
        {
            RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ProdSvcBaseUri + "/api/v1/projects/1001158/surveyedsurfaces/111/delete",
                "GET", RestClientConfig.JsonMediaType, null);
        }
    }
}