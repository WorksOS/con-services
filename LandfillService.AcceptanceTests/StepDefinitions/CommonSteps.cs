using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LandfillService.WebApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace LandfillService.AcceptanceTests.StepDefinitions
{
    public class Config
    {
        public static string ServiceUrl = "http://localhost:59674/api/v1/";
        public static Dictionary<string, Credentials> credentials = new Dictionary<string, Credentials>()
        {
            {"goodCredentials", new Credentials { userName = "akorban", password = "Bullshit1!" } },
            {"badCredentials", new Credentials { userName = "akorban", password = "badpassword" } }
        };
        //public static Credentials goodCredentials = new Credentials() { userName = "akorban", password = "Bullshit1!" };
        //public static Credentials badCredentials = new Credentials() { userName = "akorban", password = "badpassword" };
    }

    [Binding]
    public class CommonSteps
    {
        protected HttpClient httpClient;
        protected HttpResponseMessage response;
        protected string sessionId;

        protected CommonSteps()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("SessionID", sessionId);
        }

        protected HttpResponseMessage Login(Credentials credentials)
        {
            response = httpClient.PostAsJsonAsync(Config.ServiceUrl + "users/login", credentials).Result;
            System.Diagnostics.Debug.WriteLine(response.ToString());
            sessionId = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
            return response;
        }

        [StepDefinition("login (.+)")]
        public void WhenLogin(string credKey)
        {
            Login(Config.credentials[credKey]);
            //ScenarioContext.Current.Pending();
        }

        [Then(@"match response \(\w+ (.+)\)")]
        public void ThenMatchCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, (int)response.StatusCode);

            //ScenarioContext.Current.Pending();
        }

        [Then(@"not \$ null response")]
        public void ThenNotNullResponse()
        {
            Assert.IsTrue(response.Content.ReadAsStringAsync().Result.Length > 0);
        }

        [StepDefinition(@"getProjects")]
        public async void WhenGetProjects()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            var projects = await response.Content.ReadAsAsync<Project[]>();
            Assert.IsTrue(projects.Length > 0);
            System.Diagnostics.Debug.WriteLine(response.ToString());            
        }

    }
}
