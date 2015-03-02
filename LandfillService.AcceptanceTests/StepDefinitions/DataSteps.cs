using LandfillService.WebApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "Data")]
    public class DataSteps : CommonSteps
    {
        [Given(@"getData \(Project (.*)\)")]
        public async void GivenGetData(int projectId)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            var projectData = await response.Content.ReadAsAsync<DayEntry[]>();
            Assert.IsTrue(projectData.Length > 0);
            System.Diagnostics.Debug.WriteLine(response.ToString());
        }

    }
}
