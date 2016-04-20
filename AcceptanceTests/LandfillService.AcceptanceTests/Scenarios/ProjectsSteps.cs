﻿using System;
using System.Linq;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests.Models.Landfill;
using AutomationCore.API.Framework.Library;
using LandfillService.AcceptanceTests.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LandfillService.AcceptanceTests.Utils;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class ProjectsSteps
    {
        List<Project> projects;
        ProjectData data;

        [When(@"I try to get a list of all projects")]
        public void WhenITryToGetAListOfAllProjects()
        {
            string response = RestClientUtil.DoHttpRequest(Config.LandfillBaseUri, "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            projects = JsonConvert.DeserializeObject<List<Project>>(response);
        }
        
        [Then(@"the project '(.*)' is in the list")]
        public void ThenTheProjectIsInTheList(string project)
        {
            Assert.IsTrue(projects.Exists(p => p.name == project), "Project not found.");
        }

        [When(@"I try to get data for project '(.*)'")]
        public void WhenITryToGetDataForProject(string projName)
        {
            Project project = ProjectsUtils.GetProjectDetails(projName);

            string response = RestClientUtil.DoHttpRequest(string.Format("{0}/{1}", Config.LandfillBaseUri, project.id), "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            data = JsonConvert.DeserializeObject<ProjectData>(response);
        }

        [Then(@"the response contains data for the past two years")]
        public void ThenTheResponseContainsDataForThePastTwoYears()
        {
            Assert.IsTrue(data.entries.ToList().Count == 730 || data.entries.ToList().Count == 731, 
                "Incorrect number of project data entries.");
        }
    }
}
