using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TechTalk.SpecFlow;
using LandfillService.WebApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LandfillService.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "Data")]
    [TestClass()]
    public class DataSteps 
    {
        public double randomWeight;
        public static DateTime dateFiveDaysAgo = DateTime.UtcNow.AddDays(-5);
        public static DateTime dateYesterday = DateTime.UtcNow.AddDays(-1);
        public static DateTime dateToday = DateTime.UtcNow;
        public static DateTime dateTomorrow = DateTime.UtcNow.AddDays(1);
        protected HttpClient httpClient;
        protected HttpResponseMessage response;
        protected string sessionId;

        #region Initialise
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
            sessionId = response.Content.ReadAsStringAsync().Result.Replace("\"", "");
            return response;
        }

        [TestCleanup()]
        public void TestCleanup() 
        {
            httpClient.Dispose();
        }

        #endregion

        #region Private methods 
        /// <summary>
        /// Set up the weight for one day
        /// </summary>
        /// <param name="oneDayDate">The date you want the weight set up for</param>
        /// <returns>One Weightentry</returns>
        private WeightEntry[] SetUpOneWeightForOneDay(DateTime oneDayDate,double weight)
        {
            WeightEntry[] weightForOneDay = new WeightEntry[] 
            { 
                new WeightEntry (){date = oneDayDate, weight = weight}
            };
            return weightForOneDay;
        }

        /// <summary>
        /// Set up the wait for one day with a random weight
        /// </summary>
        /// <param name="oneDayDate">A valid date</param>
        /// <returns>One random weight entry</returns>
        private WeightEntry[] SetUpOneWeightForOneDay(DateTime oneDayDate)
        {
            // Set the weights 
            Random random = new Random();
            randomWeight = random.Next(3444, 5000);

            WeightEntry[] weightForOneDay = new WeightEntry[] 
            { 
                new WeightEntry (){date = oneDayDate, weight = randomWeight}
            };
            return weightForOneDay;
        }

        /// <summary>
        /// Sets up 5 weights that are loaded up to web service
        /// </summary>
        /// <returns>Five entries of in array of weight entry</returns>
        private WeightEntry[] SetUpFiveWeightsForUpload()
        {
            // Set the weights 
            Random random = new Random();
            randomWeight = random.Next(5200, 5300);

            WeightEntry[] weightForFiveDays = new WeightEntry[] 
            { 
                new WeightEntry (){date = DateTime.UtcNow.AddDays(-11), weight = randomWeight},
                new WeightEntry (){date = DateTime.UtcNow.AddDays(-10), weight = randomWeight}, 
                new WeightEntry (){date = DateTime.UtcNow.AddDays(-9), weight = randomWeight}, 
                new WeightEntry (){date = DateTime.UtcNow.AddDays(-8), weight = randomWeight}, 
                new WeightEntry (){date = DateTime.UtcNow.AddDays(-7), weight = randomWeight} 
            };
            return weightForFiveDays;
        }


        /// <summary>
        /// Sets up 1 weights and load upthat are loaded up to web service
        /// </summary>
        /// <returns>Five entries of in array of weight entry</returns>
        private WeightEntry[] SetUpFiveWeightsForUpload(DateTime specificDate, double specificWeight)
        {
            WeightEntry[] weightForOneDay = new WeightEntry[] 
            { 
                new WeightEntry (){date = specificDate, weight = specificWeight}
            };
            return weightForOneDay;
        }

        /// <summary>
        /// Check the dayEntry is for the correct date and it has a weight and density
        /// </summary>
        /// <param name="expectedVolume"></param>
        /// <param name="dateOfDensityCheck"></param>
        /// <param name="dayEntry"></param>
        private static void CheckTheDayEntryIsValid(double expectedVolume, DateTime dateOfDensityCheck, DayEntry dayEntry)
        {
            if (dayEntry.date.ToShortDateString() != dateOfDensityCheck.ToShortDateString())
            {
                Assert.Fail("Did not find the posted date:" + dateOfDensityCheck.ToShortDateString() +
                            " or date in DB:" + dayEntry.date.ToShortDateString());
            }
            else
            {
                if (dayEntry.weight == 0)
                { Assert.Fail("Weight is zero so density cannot be calculated."); }

                if (dayEntry.density == 0)
                { Assert.Fail("Density is zero so it cannot be compared."); }

                CalculateDensityAndCompare(expectedVolume, dayEntry);
            }
        }
        /// <summary>
        /// Calculate the density
        /// </summary>
        /// <param name="expectedVolume"></param>
        /// <param name="dayEntry"></param>
        private static void CalculateDensityAndCompare(double expectedVolume, DayEntry dayEntry)
        {
            const double POUNDS_PER_TON = 2000.0;
            const double M3_PER_YD3 = 0.7645555;
            double calculatedDensity = dayEntry.weight * POUNDS_PER_TON * M3_PER_YD3 / expectedVolume;

            if (Math.Round(dayEntry.density, 4) != Math.Round(calculatedDensity, 4))
            {
                Assert.Fail("Density is not as expected. density from response:" + dayEntry.density +
                            " does not equal expected:" + calculatedDensity);
            }
        }

        /// <summary>
        /// Add a weight to a project for a date
        /// </summary>
        /// <param name="projectId">Valid project id</param>
        /// <param name="dateOfWeight">A date between today and two years ago</param>
        private void AddAWeightToAProjectForADay(int projectId, DateTime dateOfWeight)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects/" + projectId + "/weights"), Method = HttpMethod.Post };
            request.Headers.Add("SessionID", sessionId);

            if (sessionId.Contains("Code"))
            {
                Assert.Inconclusive("Unable to establish a session. Check MySQL database connection");
                ScenarioContext.Current.Pending();
            }
            request.Content = new StringContent(JsonConvert.SerializeObject(SetUpOneWeightForOneDay(dateOfWeight)), Encoding.UTF8, "application/json");
            response = httpClient.SendAsync(request).Result;
        }

        /// <summary>
        /// Check the weight is in the response from date in question
        /// </summary>
        /// <param name="dateOfCheck">Date to check in the response</param>
        private void CheckTheWeightHasBeenAddedToTheProject(DateTime dateOfCheck)
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(response.Content.ReadAsStringAsync().Result);
            var dayEntry = from day in projectData.entries
                           where day.date >= dateOfCheck.AddDays(-2) && day.weight == randomWeight
                           select day;

            if (!dayEntry.Any())
            {
                Log.WriteLineFormat("Did not find any entries for {0} for the >= date {1}", randomWeight, dateOfCheck.AddDays(-2));
                Assert.Fail("Did not find any weights for the one it just posted. Weight:" + randomWeight + " for Date:" + dateOfCheck.ToShortDateString());
            }
            else
            {
                if (dayEntry.First<DayEntry>().weight != randomWeight || dayEntry.First<DayEntry>().date.ToShortDateString() != dateOfCheck.ToShortDateString())
                {
                    Log.WriteLineFormat("Did not find the entries with the same weight as {0} for the >= date {1}", randomWeight, dateOfCheck.AddDays(-2));
                    Assert.Fail("Did not find the weight it just posted. Posted weight:" + randomWeight + " weight in DB:" + dayEntry.First<DayEntry>().weight +
                                " or for posted date:" + dateOfCheck.ToShortDateString() + " or date in DB:" + dayEntry.First<DayEntry>().date.ToShortDateString());
                }
            }
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

        [Then(@"not \$ null response")]
        public void ThenNotNullResponse()
        {
            Assert.IsTrue(response.Content.ReadAsStringAsync().Result.Length > 0);
        }

        [When(@"get list of projects")]
        public async void WhenGetListOfProjects()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            // Try and get the projects. Should cause exception
            var projects = await response.Content.ReadAsAsync<Project[]>();
            List<Project> allProjects = JsonConvert.DeserializeObject<List<Project>>(response.Content.ReadAsStringAsync().Result);
            Assert.IsNotNull(allProjects, " Projects should not be available after logging out");
        }

        [Given(@"Get a list of all projects")]
        public async void GivenGetAListOfAllProjects()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            var projects = await response.Content.ReadAsAsync<Project[]>();
        }

        [Then(@"check the \(Project (.*)\) is in the list")]
        public void ThenCheckTheProjectIsInTheList(int projectId)
        {
            List<Project> allProjects = JsonConvert.DeserializeObject<List<Project>>(response.Content.ReadAsStringAsync().Result);
            if (allProjects != null)
            {                
                if(allProjects.Any(prj => prj.id != projectId))
                {
                    Assert.Fail("Project " + projectId + " does not exist ");
                }
            }
            else
            {
                Assert.Fail("Cannot find any projects ");
            }
        }

        [Given(@"Get project data for project \((.*)\)")]
        public async void GivenGetProjectDataForProject(int projectId)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects/" + projectId), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            var projectData = await response.Content.ReadAsAsync<ProjectData>();           
        }

        [Then(@"check there is (.*) days worth of data for project \((.*)\)")]
        public void ThenCheckThereIsDaysWorthOfDataForProject(int dayslimit, int projectId)
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(response.Content.ReadAsStringAsync().Result);
            if (projectData != null)
            {
                Assert.IsTrue(projectData.entries.Count() > dayslimit, "There wasn't " + dayslimit + " days worth of data for project " + projectId + ". Entries = " + projectData.entries.Count());
            }
            else
            {
                Assert.Fail("There wasn't any data for project " + projectId);
            }
        }

        [When(@"adding a random weight for project \((.*)\) five days ago")]
        public void WhenAddingAWeightTonnesForProjectFiveDaysAgo(int projectId)
        {
            AddAWeightToAProjectForADay(projectId, dateFiveDaysAgo);
        }

        [Then(@"check the random weight has been added to the project \((.*)\) for five days ago")]
        public void ThenCheckTheWeightTonnesHasBeenAddedToTheProjectForFiveDaysAgo(int projectId)
        {
            CheckTheWeightHasBeenAddedToTheProject(dateFiveDaysAgo);
        }

        [When(@"adding five random weights for project \((.*)\) ten days ago")]
        public void WhenAddingFiveRandomWeightsForProjectTenDaysAgo(int projectId)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects/" + projectId + "/weights"), Method = HttpMethod.Post };
            request.Headers.Add("SessionID", sessionId);
            if (sessionId.Contains("Code"))
            {
                Assert.Inconclusive("Unable to establish a session. Check MySQL database connection");
            }

            WeightEntry[] weightForFiveDays = SetUpFiveWeightsForUpload();
            request.Content = new StringContent(JsonConvert.SerializeObject(weightForFiveDays), Encoding.UTF8, "application/json");
            response = httpClient.SendAsync(request).Result;
        }

        [Then(@"check the five random weights has been added each day to the project \((.*)\)")]
        public void ThenCheckTheFiveRandomWeightsHasBeenAddedEachDayToTheProject(int projectId)
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(response.Content.ReadAsStringAsync().Result);
            var fiveDayEntries = from dayEntryWeight in projectData.entries
                                 where dayEntryWeight.date >= DateTime.UtcNow.AddDays(-12) && dayEntryWeight.date <= DateTime.UtcNow.AddDays(-7)
                                 select dayEntryWeight.weight;

            foreach (var dayEntryWeight in fiveDayEntries)
            {
                if (dayEntryWeight != randomWeight)
                {
                    Assert.Fail("Weight retrieve from response:" + dayEntryWeight + " does not equal expected:" + randomWeight);
                }
            } 
        }

        [Then(@"check the density is \((.*)\) for the date \((.*)\)")]
        public void ThenCheckTheDensityIsForTheDate(double expectedDensity, DateTime dateOfDensityCheck)
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(response.Content.ReadAsStringAsync().Result);
            var dayEntry = from day in projectData.entries
                           where day.date.ToShortDateString() == dateOfDensityCheck.ToShortDateString()
                           select day.density;

            if ( Math.Round(dayEntry.First(),4) !=  Math.Round(expectedDensity,4))
            {
                Assert.Fail("density retrieve from response:" + Math.Round(dayEntry.First(), 4) + " does not equal expected:" + Math.Round(expectedDensity, 4));
            } 
        }

        [When(@"updating a weight \((.*)\) tonnes for project \((.*)\) for date \((.*)\)")]
        public void WhenUpdatingAWeightTonnesForProjectForDate(double weight, int projectId, DateTime dateWeightUpdate)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects/" + projectId + "/weights"), Method = HttpMethod.Post };
            request.Headers.Add("SessionID", sessionId);
            if (sessionId.Contains("Code"))
            {
                Assert.Inconclusive("Unable to establish a session. Check MySQL database connection");
            }
            request.Content = new StringContent(JsonConvert.SerializeObject(SetUpOneWeightForOneDay(dateWeightUpdate,weight)), Encoding.UTF8, "application/json");
            response = httpClient.SendAsync(request).Result;
        }

        [Then(@"check the density is calculated with a volume of \((.*)\) for the date \((.*)\)")]
        public void ThenCheckTheDensityIsCalculatedAsForTheDate(double expectedVolume, DateTime dateOfDensityCheck)
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(response.Content.ReadAsStringAsync().Result);
            var dayEntry = from day in projectData.entries
                           where day.date.ToShortDateString() == dateOfDensityCheck.ToShortDateString()
                           select day;

            if (!dayEntry.Any())
            {
                Assert.Fail("Did not find any entries in the for selected date:" + dateOfDensityCheck.ToShortDateString());
            }
            else
            {
                CheckTheDayEntryIsValid(expectedVolume, dateOfDensityCheck, dayEntry.First<DayEntry>());
            }
        }

        // Expanding the tests to test ranges and negative tests

        [When(@"adding a random weight for project \((.*)\) yesterday")]
        public void WhenAddingARandomWeightForProjectYesterday(int projectId)
        {
            Log.Write("Adding a weight for date yesterday: " + dateYesterday.ToString());
            AddAWeightToAProjectForADay(projectId, dateYesterday);
        }

        [Then(@"check the random weight has been added to the project \((.*)\) for yesterday")]
        public void ThenCheckTheRandomWeightHasBeenAddedToTheProjectForYesterday(int projectId)
        {
            CheckTheWeightHasBeenAddedToTheProject(dateYesterday);
        }

        [When(@"adding a random weight for project \((.*)\) today")]
        public void WhenAddingARandomWeightForProjectToday(int projectId)
        {
            Log.Write("Adding a weight for today: " + dateToday.ToString());
            AddAWeightToAProjectForADay(projectId, dateToday);
        }

        [Then(@"check the random weight has been added to the project \((.*)\) for today")]
        public void ThenCheckTheRandomWeightHasBeenAddedToTheProjectForToday(int projectId)
        {
            CheckTheWeightHasBeenAddedToTheProject(dateToday);
        }

        [When(@"adding a random weight for project \((.*)\) tomorrow")]
        public void WhenAddingARandomWeightForProjectTomorrow(int projectId)
        {
            Log.Write("Adding a weight for date tomorrow: " + dateTomorrow.ToString());
            AddAWeightToAProjectForADay(projectId, dateTomorrow);
        }

        [Then(@"check the random weight has been added to the project \((.*)\) for tomorrow")]
        public void ThenCheckTheRandomWeightHasBeenAddedToTheProjectForTomorrow(int projectId)
        {            
            CheckTheWeightHasBeenAddedToTheProject(dateTomorrow);
        }

        #endregion
    }
}
