using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using LandfillService.AcceptanceTests.Helpers;
using LandfillService.WebApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding, Scope(Feature = "Login")]
    [TestClass()]
    public class LoginSteps 
    {
        private HttpClient httpClient;
        private HttpResponseMessage response;
        private string sessionId;
        private string responseParse;
        private string logonkey;

        #region initialise

        [ClassInitialize()]
        public void DataStepsInitialize() { }

        [ClassCleanup()]
        public static void DataStepsCleanup() { }

        [TestInitialize]
        protected HttpResponseMessage Login(Credentials credentials)
        {
            try
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("SessionID", sessionId);
                response = httpClient.PostAsJsonAsync(Config.serviceUrl + "users/login", credentials).Result;
                responseParse = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
                if (responseParse.Length > 32)
                {
                    sessionId = responseParse.Substring(0, 32);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to login with creditials. Exception:" + ex.Message);
            }
            return response;
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            httpClient.Dispose();
        }

        #endregion


        #region Scenairo tests

        [StepDefinition("login (.+)")]
        public void WhenLogin(string credKey)
        {
            Login(Config.credentials[credKey]);
        }

        [Then(@"match response \(\w+ (.+)\)")]
        public void ThenMatchCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, (int)response.StatusCode, "HTTP response status codes not matching expected");
        }

        [Then(@"not null response")]
        public void ThenNotNullResponse()
        {
            Assert.IsTrue(response.Content.ReadAsStringAsync().Result.Length > 0);
        }

        [When(@"not null response")]
        public void WhenNotNullResponse()
        {
            Assert.IsTrue(response.Content.ReadAsStringAsync().Result.Length > 0);
        }

        [When(@"get list of projects")]
        public void WhenGetListOfProjects()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            List<Project> allProjects = JsonConvert.DeserializeObject<List<Project>>(response.Content.ReadAsStringAsync().Result);
            Assert.IsNotNull(allProjects, "The list of projects cannot be found. They should be available");
        }

        [When(@"try get list of projects")]
        public void WhenTryGetListOfProjects()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode) 
                {return;}

            Assert.Fail("The list of projects should not be available but the call was successful:" + response.Content.ReadAsStringAsync().Result);
        }


        [When("logout")]
        public void WhenLogout()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "users/logout"), Method = HttpMethod.Post };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [When(@"use badSession")]
        public void WhenUseBadSession()
        {
            sessionId = "<bad_session>";
        }

        [Then("match response SessionId")]
        public void ThenMatchSessionId()
        {            
            Assert.IsTrue(response.IsSuccessStatusCode && new Regex(@"\w{32}").IsMatch(response.Content.ReadAsStringAsync().Result));
        }


        /// <summary>
        /// This is for logging in using a token. This is the way they will logon using visionlink.
        /// </summary>
        [Given(@"I have retrieve a token from the project monitoring api")]
        public void GivenIHaveRetrieveATokenFromTheProjectMonitoringApi()
        {            
            LogonKey logonTokenKey = new LogonKey();
            logonkey = logonTokenKey.GetKeyToken();
        }

        [When(@"I logon with the token")]
        public void WhenILogonWithTheToken()
        {
            VlCredentials vlcredentials = new VlCredentials { userName = ConfigurationManager.AppSettings["UserName"], key = logonkey };
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("SessionID", sessionId);
            response = httpClient.PostAsJsonAsync(Config.serviceUrl + "users/login/vl", vlcredentials).Result;
            responseParse = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
            if (responseParse.Length > 32)
            {
                sessionId = responseParse.Substring(0, 32);
            }
           // sessionId = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
            //Assert.IsTrue(sessionId.Length == 32, "Logon test has failed: Response status cd:" + response.StatusCode + " and session id:" + sessionId);
        }
        #endregion
    }
}
