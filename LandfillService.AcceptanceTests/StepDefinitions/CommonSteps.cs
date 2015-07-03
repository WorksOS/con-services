using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using System.Net.Http;
using System.Net.Http.Headers;
using LandfillService.WebApi.Models;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LandfillService.AcceptanceTests.StepDefinitions
{
    public class Config
    {
        //public static string ServiceUrl = "http://localhost:59674/api/v1/";
        public static string ServiceUrl = "http://10.210.246.188/LandfillService/api/v1/";
        public static Dictionary<string, Credentials> credentials = new Dictionary<string, Credentials>()
        {
            {"goodCredentials", new Credentials { userName = "dglassenbury", password = "Visionlink15_" } },
            {"invalidUsername", new Credentials { userName = "rubbish", password = "zzzzzzzzz123456" } },            
            {"badCredentials", new Credentials { userName = "akorban", password = "badpassword" } },
            {"noCredentials", new Credentials { userName = "", password = "" } }
        };
    }

    [Binding]
    public class CommonSteps
    {
        protected HttpClient httpClient;
        protected HttpResponseMessage response;
        protected string sessionId;

        protected CommonSteps()
        {
            try
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("SessionID", sessionId);
            }
            catch (Exception)
            {

            }
        }

        protected HttpResponseMessage Login(Credentials credentials)
        {
            try
            {
                response = httpClient.PostAsJsonAsync(Config.ServiceUrl + "users/login", credentials).Result;
                sessionId = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
                return response;
            }
            catch (Exception)
            {
                
            }
            return response;
        }

        [StepDefinition("login (.+)")]
        public void WhenLogin(string credKey)
        {
            Login(Config.credentials[credKey]);
        }

        [Then(@"match response \(\w+ (.+)\)")]
        public void ThenMatchCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, (int)response.StatusCode,"HTTP response status codes not matching expected");
        }

        [Then(@"not \$ null response")]
        public void ThenNotNullResponse()
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
                Assert.IsNotNull(allProjects, " Projects should not be available after logging out");
            }
            catch (Exception)
            {
                // If it has failed with an exception then the test has passed
            }
        }
    }
}
