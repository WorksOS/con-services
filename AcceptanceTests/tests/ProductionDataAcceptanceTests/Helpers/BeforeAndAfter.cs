using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using RaptorSvcAcceptTestsCommon.Utils;
using RestAPICoreTestFramework.Utils.Common;

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

        [AfterScenario("requireDummySurveyedSurface")]
        public static void DeleteDummySurveyedSurface()
        {
            RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ProdSvcBaseUri + "/api/v1/projects/1001158/surveyedsurfaces/111/delete",
                "GET", RestClientConfig.JsonMediaType, null);
        }
    }
}
