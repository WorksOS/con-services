using System.Net.Http;
using System.Net.Http.Headers;
using LandfillService.AcceptanceTests.Helpers;
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


        private static void Login(Credentials credentials)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("SessionID", sessionId);
            string uri; // = "https://dev-mobile.vss-eng.com/foreman/Secure/ForemanSvc.svc/Login";
            uri = Config.LandfillBaseUri + "users/login";
            response = httpClient.PostAsJsonAsync(uri, credentials).Result;  
            responseParse = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
            Assert.IsFalse(responseParse.Contains("<html>"), "Failed to login - Is Foreman unavailable? - Response:" + responseParse);
            Assert.IsFalse(responseParse.Contains("{"),  "Failed to login - Is Foreman unavailable? - Response:" + responseParse);        
            sessionId = GetSessionIdFromResponse(responseParse);
        }


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
