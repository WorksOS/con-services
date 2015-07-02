using LandfillService.WebApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TechTalk.SpecFlow;


namespace LandfillService.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "Login")]
    public class LoginSteps : CommonSteps
    {
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
    }
}
