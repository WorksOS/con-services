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


namespace LandfillService.AcceptanceTests.StepDefinitions
{
    public class Config
    {
        public const string ServiceUrl = "http://localhost:59674/";
    }

    [Binding, Scope(Feature = "Login")]
    public class LoginSteps
    {
        private string uri;
        private HttpResponseMessage response;

        [Given("(.+)")]
        public void GivenUri(string uri)
        {
            this.uri = uri;
            //ScenarioContext.Current.Pending();
        }

        [When("login '(.+)' '(.+)'")]
        public void WhenLogin(string userName, string password)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                response = httpClient.PostAsJsonAsync(Config.ServiceUrl + uri, new Credentials() { userName = userName, password = password }).Result;
                System.Diagnostics.Debug.WriteLine(response.ToString());
            }
            //ScenarioContext.Current.Pending();
        }

        [Then("match response SessionId")]
        public void ThenMatchSessionId()
        {
            Assert.IsTrue(response.IsSuccessStatusCode && new Regex(@"\w{32}").IsMatch(response.Content.ReadAsStringAsync().Result));
        }

        [Then(@"match response \(Error (.+)\)")]
        public void ThenMatchError(int expectedCode)
        {
            Assert.AreEqual(expectedCode, (int)response.StatusCode);

            //ScenarioContext.Current.Pending();
        }

    }
}
