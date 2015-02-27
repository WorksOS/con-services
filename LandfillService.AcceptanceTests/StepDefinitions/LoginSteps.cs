using LandfillService.WebApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    [Binding, Scope(Feature = "Login")]
    public class LoginSteps
    {
        private string uri;
        private HttpClient httpClient;
        private HttpResponseMessage response;
        private string sessionId;

        LoginSteps()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private HttpResponseMessage Login(Credentials credentials)
        {
            response = httpClient.PostAsJsonAsync(Config.ServiceUrl + uri, credentials).Result;
            System.Diagnostics.Debug.WriteLine(response.ToString());
            return response;
        }

        [Given("(.+)")]
        public void GivenUri(string uri)
        {
            this.uri = uri;
            //ScenarioContext.Current.Pending();
        }


        [When("login (.+)")]
        public void WhenLogin(string credKey)
        {
            Login(Config.credentials[credKey]);
            //ScenarioContext.Current.Pending();
        }

        [When("logout")]
        public void WhenLogout()
        {
            response = Login(Config.credentials["goodCredentials"]);

            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "users/logout"), Method = HttpMethod.Post };
            sessionId = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
            request.Headers.Add("SessionID", sessionId);

            response = httpClient.SendAsync(request).Result;

            System.Diagnostics.Debug.WriteLine(response.ToString());
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Task.Delay(10000).Wait();

            //ScenarioContext.Current.Pending();
        }

        [When(@"getProjects")]
        public void WhenGetProjects()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;

            System.Diagnostics.Debug.WriteLine(response.ToString());            
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

        [Then(@"match response \(\w+ (.+)\)")]
        public void ThenMatchError(int expectedCode)
        {
            Assert.AreEqual(expectedCode, (int)response.StatusCode);

            //ScenarioContext.Current.Pending();
        }

        [Then(@"not \$ null response")]
        public void ThenNotNullResponse()
        {
            Assert.IsTrue(response.Content.ReadAsStringAsync().Result.Length > 0);
        }

    }
}
