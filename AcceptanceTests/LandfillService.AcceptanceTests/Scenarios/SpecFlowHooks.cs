using System.Net.Http;
using System.Net.Http.Headers;
using LandfillService.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class SpecFlowHooks
    {
        public static string sessionId = string.Empty;

        protected static HttpClient httpClient;
        protected static HttpResponseMessage response;
        protected static string responseParse;

        public static string GetSessionIdFromResponse(string inResponse)
        {
            if (inResponse.Length < 32)
                { return inResponse; }
            return inResponse.Substring(0, 32);
        }

        [BeforeTestRun]
        public static void Before()
        {
            //Login(Config.credentials["goodCredentials"]);
        }

        [AfterTestRun]
        public static void After()
        {

        }

        [BeforeFeature]
        public static void BeforeFeature()
        {

        }

        [AfterFeature]
        public static void AfterFeature()
        {

        }

        [BeforeScenario]
        public void BeforeScenario()
        {

        }

        [AfterScenario]
        public void AfterScenario()
        {

        }
    }
}
