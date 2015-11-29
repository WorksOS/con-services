using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using LandfillService.AcceptanceTests.Helpers;
using LandfillService.WebApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding, Scope(Feature = "Data")]
    [TestClass()]
    public class DataSteps 
    {
        protected HttpClient HttpClient;
        protected HttpResponseMessage Response;
        protected string ResponseParse;
        private string sessionId;
        private UnitsTypeEnum unitsEnum;
        private double weightAdded;
        private int projectId;
        private string timeZoneFromProject;
        private Project currentProject;

        private const double POUNDS_PER_TON = 2000.0;
        private const double M3_PER_YD3 = 0.7645555;
        private const double EPSILON = 0.001;

        private readonly StepSupport stepSupport = new StepSupport();

        #region Initialise
        [ClassInitialize()]
        public void DataStepsInitialize() { }

        [ClassCleanup()]
        public static void DataStepsCleanup() { }

        [TestInitialize]
        protected HttpResponseMessage Login(Credentials credentials)
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.Add("SessionID", sessionId);       
            Response = HttpClient.PostAsJsonAsync(Config.serviceUrl + "users/login", credentials).Result;
            ResponseParse = Response.Content.ReadAsStringAsync().Result.Replace("\"", "");
            Assert.IsFalse(ResponseParse.Contains("<html>"),"Failed to login - Is Foreman unavailable?");
            sessionId = stepSupport.GetSessionIdFromResponse(ResponseParse);
            unitsEnum = stepSupport.GetWeightUnitsFromResponse(ResponseParse);
            return Response;
        }

        [TestCleanup()]
        public void TestCleanup() 
        {
            HttpClient.Dispose();
        }

        #endregion

        #region Private methods 

        /// <summary>
        /// Add a weight to a project for a date
        /// </summary>
        /// <param name="idFromProject">Valid project id</param>
        /// <param name="dateOfWeight">A date between today and two years ago</param>
        private void AddAWeightToAProjectForADay(int idFromProject, DateTime dateOfWeight)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects/" + idFromProject + "/weights"), Method = HttpMethod.Post };
            request.Headers.Add("SessionID", sessionId);
            Assert.IsFalse(sessionId.Contains("Code"), "Unable to establish a session. Check MySQL database connection");
            WeightEntry[] weightEntry = stepSupport.SetUpOneWeightForOneDay(dateOfWeight);
            weightAdded = weightEntry[0].weight;
            request.Content = new StringContent(JsonConvert.SerializeObject(weightEntry), Encoding.UTF8, "application/json");
            Response = HttpClient.SendAsync(request).Result;
        }

        /// <summary>
        /// Check the weight is in the response from date in question
        /// </summary>
        /// <param name="dateOfCheck">EntryDate to check in the response</param>
        private void CheckTheWeightHasBeenAddedToTheProject(DateTime dateOfCheck)
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(Response.Content.ReadAsStringAsync().Result);
            var dayEntry = from day in projectData.entries
                           where day.date >= dateOfCheck.AddDays(-2).Date && Math.Abs(day.weight - weightAdded) < EPSILON
                           select day;

            // There is no day entry
            Assert.IsNotNull(dayEntry, "Did not find any weights for the one it just posted. Weight:" + weightAdded + " for EntryDate:" + dateOfCheck.ToShortDateString());
            // Weights are not equal
            var dayEntries = dayEntry as DayEntry[] ?? dayEntry.ToArray();
            Assert.AreEqual(dayEntries.First().weight, weightAdded, "Did not find the weight it just posted. Posted weight:" + weightAdded + " weight in DB:" + dayEntries.First().weight);
            // EntryDate's are equal
            Assert.AreEqual(dayEntries.First().date.ToShortDateString(), dateOfCheck.ToShortDateString(), " Posted not equal to database. Expected: " + dateOfCheck.ToShortDateString() + " Actual date in DB:" + dayEntries.First().date.ToShortDateString());
        }

        /// <summary>
        /// Get all the entries for the past two 
        /// </summary>
        private void GetProjectData()
        {
            var requestdata = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects/" + projectId), Method = HttpMethod.Get };
            requestdata.Headers.Add("SessionID", sessionId);
            Response = HttpClient.SendAsync(requestdata).Result;
        }

        /// <summary>
        /// Set the project details for the current project
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="allProjects"></param>
        /// <returns></returns>
        private bool GetProjectDetailsFromListOfProjects(string projectName, List<Project> allProjects)
        {
            foreach (var proj in allProjects)
            {
                if (proj.name.Trim().ToLower() == projectName.Trim().ToLower())
                {
                    currentProject = proj;
                    projectId = Convert.ToInt32(proj.id);
                    timeZoneFromProject = proj.timeZoneName;
                    GetProjectData();
                    return true;
                }
            }
            return false;
        }

 
        /// <summary>
        /// Retrieve all the values from the mySQL database that have a volume and a weight. Then return the day we are after.
        /// </summary>
        /// <param name="inDate">EntryDate searching for</param>
        /// <returns>Day of from entries table - Has the weight, volume and density</returns>
        private DayEntryAll GetEntriesFromMySqlDb(DateTime inDate)
        {
           // LandFillMySqlDb landfillMySqlDb = new LandFillMySqlDb();
            var allEntries = LandFillMySqlDb.GetEntries(currentProject, unitsEnum);
            foreach (var day in allEntries)
            {
                if (inDate.Date == day.EntryDate)
                {
                    return day;
                }
            }
            return null;
        }

        #endregion 

        #region Scenairo tests

        [StepDefinition("login (.+)")]
        public void WhenLogin(string credKey)
        {
            Login(Config.credentials[credKey]);
        }

        [Given(@"Get Project data for '(.*)'")]
        public void GivenGetProjectDataFor(string projectName)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            Response = HttpClient.SendAsync(request).Result;
            // Try and get the projects. Should cause exception
        //    var projects = await response.Content.ReadAsAsync<Project[]>();
            List<Project> allProjects = JsonConvert.DeserializeObject<List<Project>>(Response.Content.ReadAsStringAsync().Result);            
            Assert.IsTrue(GetProjectDetailsFromListOfProjects(projectName, allProjects),"Project " + projectName + " does not exist list. Number of projects in list is " + allProjects.Count);
        }

        [Then(@"match response \(\w+ (.+)\)")]
        public void ThenMatchCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, (int)Response.StatusCode, "HTTP response status codes not matching expected");
        }

        [Given(@"Get a list of all projects")]
        public void GivenGetAListOfAllProjects()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            Response = HttpClient.SendAsync(request).Result;
           // var projects = await response.Content.ReadAsAsync<Project[]>();
        }

        [Then(@"check the project '(.*)' is in the list")]
        public void ThenCheckTheProjectIsInTheList(string testProjectName)
        {
            List<Project> allProjects = JsonConvert.DeserializeObject<List<Project>>(Response.Content.ReadAsStringAsync().Result);
            Assert.IsNotNull(allProjects, "There are no projects in the project list");
            if (allProjects.Any(prj => prj.name == testProjectName))
                 { return; }
            Assert.Fail("Project " + testProjectName + " does not exist list. Number of projects in list is " + allProjects.Count);
        }

        [Then(@"check there is (.*) days worth of data")]
        public void ThenCheckThereIsDaysWorthOfData(int dayslimit)
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(Response.Content.ReadAsStringAsync().Result);
            if (projectData != null)
            {
                Assert.IsTrue(projectData.entries.Count() > dayslimit, "There wasn't " + dayslimit + " days worth of data for project " + projectId + ". Entries = " + projectData.entries.Count());
            }
            else
            {
                Assert.Fail("There wasn't any data for project " + projectId);
            }
        }

        [Then(@"compare the subscription expiry days left to mySql database")]
        public void ThenCompareTheSubscriptionExpiryDaysLeftToMySqlDatabase()
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(Response.Content.ReadAsStringAsync().Result);
            if (projectData != null)
            {
                var projectDetails = LandFillMySqlDb.GetProject((int)projectData.project.id);
                Assert.IsTrue(projectDetails.daysToSubscriptionExpiry == projectData.project.daysToSubscriptionExpiry, "The subscription expiry days left does not equal mySql database. Expected:" + projectData.project.daysToSubscriptionExpiry + " Actual:" + projectDetails.daysToSubscriptionExpiry);
            }
            else
            {
                Assert.Fail("There wasn't any data for project " + projectId);
            }
        }

        [When(@"adding a random weight for five days ago")]
        public void WhenAddingARandomWeightForFiveDaysAgo()
        {
            AddAWeightToAProjectForADay(projectId, stepSupport.GetFiveDaysAgoForTimeZone(timeZoneFromProject));
        }

        [Then(@"check the random weight has been added for five days ago")]
        public void ThenCheckTheRandomWeightHasBeenAddedForFiveDaysAgo()
        {
            CheckTheWeightHasBeenAddedToTheProject(stepSupport.GetFiveDaysAgoForTimeZone(timeZoneFromProject));
        }
        
        [When(@"adding five random weights for ten days ago")]
        public void WhenAddingFiveRandomWeightsForTenDaysAgo()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects/" + projectId + "/weights"), Method = HttpMethod.Post };
            request.Headers.Add("SessionID", sessionId);
            if (sessionId.Contains("Code"))
            {
                Assert.Inconclusive("Unable to establish a session. Check MySQL database connection");
            }

            WeightEntry[] weightForFiveDays = stepSupport.SetUpFiveWeightsForUpload(stepSupport.GetTodayForTimeZone(timeZoneFromProject));
            weightAdded = weightForFiveDays[0].weight;
            request.Content = new StringContent(JsonConvert.SerializeObject(weightForFiveDays), Encoding.UTF8, "application/json");
            Response = HttpClient.SendAsync(request).Result;
        }

        [Then(@"check the five random weights has been added each day")]
        public void ThenCheckTheFiveRandomWeightsHasBeenAddedEachDay()
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(Response.Content.ReadAsStringAsync().Result);
            var dateToday = stepSupport.GetTodayForTimeZone(timeZoneFromProject);
            var fiveDayEntries = from dayEntryWeight in projectData.entries
                                 where dayEntryWeight.date >=  dateToday.AddDays(-12) && dayEntryWeight.date <= dateToday.AddDays(-7)
                                 select dayEntryWeight.weight;

            foreach (var dayEntryWeight in fiveDayEntries)
            {
                Assert.AreEqual(dayEntryWeight, weightAdded, "Weight retrieve from response:" + dayEntryWeight + " does not equal expected:" + weightAdded);
            } 
        }


        [Then(@"check the calculated density is correct for the date \((.*)\)")]
        public void ThenCheckTheCalculatedDensityIsCorrectForTheDate(DateTime dateOfDensityCheck)
        {
            var dayOfDensityCheck = GetEntriesFromMySqlDb(dateOfDensityCheck);
            if (dayOfDensityCheck == null)
            {
                Assert.Fail("Cannot find any entry in the MySQL database for that day");
            }

            double calculatedDensity;
            if (unitsEnum == UnitsTypeEnum.Metric)
                { calculatedDensity = dayOfDensityCheck.Weight * 1000 / dayOfDensityCheck.Volume; }
            else
                { calculatedDensity = dayOfDensityCheck.Weight * M3_PER_YD3 * POUNDS_PER_TON / dayOfDensityCheck.Volume; }

            var projectData = JsonConvert.DeserializeObject<ProjectData>(Response.Content.ReadAsStringAsync().Result);
            var dayEntry = from day in projectData.entries
                           where day.date.ToShortDateString() == dateOfDensityCheck.ToShortDateString()
                           select day.density;

            var dayentries = dayEntry as double[] ?? dayEntry.ToArray();
            Assert.AreEqual(Math.Round(dayentries.First(), 4), Math.Round(calculatedDensity, 4), "density retrieve from response:" + Math.Round(dayentries.First(), 4) + " does not equal calculated expected:" + Math.Round(calculatedDensity, 4) + " The volume is: "
                            + dayOfDensityCheck.Volume + " and weight is:" + dayOfDensityCheck.Weight);

        }

        [When(@"updating a weight \((.*)\) tonnes for date \((.*)\)")]
        public void WhenUpdatingAWeightTonnesForDate(double weight, DateTime dateWeightUpdate)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects/" + projectId + "/weights"), Method = HttpMethod.Post };
            request.Headers.Add("SessionID", sessionId);
            if (sessionId.Contains("Code"))
            {
                Assert.Inconclusive("Unable to establish a session. Check MySQL database connection");
            }
            request.Content = new StringContent(JsonConvert.SerializeObject(stepSupport.SetUpOneWeightForOneDay(dateWeightUpdate, weight)), Encoding.UTF8, "application/json");
            Response = HttpClient.SendAsync(request).Result;
        }

        [When(@"adding a random weight for yesterday")]
        public void WhenAddingARandomWeightForYesterday()
        {
            AddAWeightToAProjectForADay(projectId, stepSupport.GetYesterdayForTimeZone(timeZoneFromProject));
        }

        [Then(@"check the random weight has been added for yesterday")]
        public void ThenCheckTheRandomWeightHasBeenAddedForYesterday()
        {
            CheckTheWeightHasBeenAddedToTheProject(stepSupport.GetYesterdayForTimeZone(timeZoneFromProject));
        }

        [When(@"adding a random weight for today")]
        public void WhenAddingARandomWeightForToday()
        {
            AddAWeightToAProjectForADay(projectId, stepSupport.GetTodayForTimeZone(timeZoneFromProject));
        }

        [When(@"adding a random weight for tomorrow")]
        public void WhenAddingARandomWeightForTomorrow()
        {
            AddAWeightToAProjectForADay(projectId, stepSupport.GetTomorrowForTimeZone(timeZoneFromProject));
        }



        #endregion
    }
}
