using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;
using LandfillService.WebApi.Models;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LandfillService.AcceptanceTests.Helpers;
using System.Text;

namespace LandfillService.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "Login")]
    [TestClass()]
    public class LoginSteps 
    {
        protected HttpClient httpClient;
        protected HttpResponseMessage response;
        protected string sessionId;
        protected string responseParse;
        protected string logonkey;
        protected string accesstoken;

        #region initialise

        [ClassInitialize()]
        public void DataStepsInitialize() { }

        [ClassCleanup()]
        public static void DataStepsCleanup() { }

        [TestInitialize]
        protected HttpResponseMessage Login(Credentials credentials)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("SessionID", sessionId);
            response = httpClient.PostAsJsonAsync(Config.ServiceUrl + "users/login", credentials).Result;
            responseParse = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
            if (responseParse.Length > 32)
            {
                sessionId = responseParse.Substring(0, 32);
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
        public async void WhenGetListOfProjects()
        {
            try
            {
                var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects"), Method = HttpMethod.Get };
                request.Headers.Add("SessionID", sessionId);
                response = httpClient.SendAsync(request).Result;

                // Try and get the projects. Should cause exception
                var projects = await response.Content.ReadAsAsync<Project[]>();
                List<Project> allProjects = JsonConvert.DeserializeObject<List<Project>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                return;
            }
            Assert.IsNotNull(" Projects should not be available and the test can see them"); 
        }

        [When("logout")]
        public void WhenLogout()
        {
         //   response = Login(Config.credentials["goodCredentials"]);
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "users/logout"), Method = HttpMethod.Post };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;

            System.Diagnostics.Debug.WriteLine(response.ToString());
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
            VlCredentials vlcredentials = new VlCredentials { userName = "dglassenbury", key = logonkey };
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("SessionID", sessionId);
            response = httpClient.PostAsJsonAsync(Config.ServiceUrl + "users/login/vl", vlcredentials).Result;
            sessionId = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
            //Assert.IsTrue(sessionId.Length == 32, "Logon test has failed: Response status cd:" + response.StatusCode + " and session id:" + sessionId);
        }
        #endregion
    }
}
