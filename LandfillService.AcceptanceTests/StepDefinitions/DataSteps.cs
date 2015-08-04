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
        public DateTime dateFiveDaysAgo;
        public DateTime dateYesterday;
        public DateTime dateToday;
        public DateTime dateTomorrow;
        protected HttpClient httpClient;
        protected HttpResponseMessage response;
        protected string responseParse;
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

        #region Private methods 

        private void SetAllDates (string projectTimeZone)
        {
            TimeZoneInfo hwZone = OlsonTimeZoneToTimeZoneInfo(projectTimeZone);
            var now = DateTime.UtcNow.Add(hwZone.BaseUtcOffset);
            // Set all the dates
            dateFiveDaysAgo = now.AddDays(-5);
            dateYesterday = now.AddDays(-1);
            dateToday = now;
            dateTomorrow = now.AddDays(1);
        }

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
            randomWeight = random.Next(2200, 3300);

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
            randomWeight = random.Next(2200, 3300);

            WeightEntry[] weightForFiveDays = new WeightEntry[] 
            { 
                new WeightEntry (){date = dateToday.AddDays(-11), weight = randomWeight},
                new WeightEntry (){date = dateToday.AddDays(-10), weight = randomWeight}, 
                new WeightEntry (){date = dateToday.AddDays(-9), weight = randomWeight}, 
                new WeightEntry (){date = dateToday.AddDays(-8), weight = randomWeight}, 
                new WeightEntry (){date = dateToday.AddDays(-7), weight = randomWeight} 
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
        private static void CheckTheDayEntryIsValid(double expectedDensity, DateTime dateOfDensityCheck, DayEntry dayEntry)
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

                CompareDensity(expectedDensity, dayEntry);
            }
        }
        /// <summary>
        /// Calculate the density
        /// </summary>
        /// <param name="expectedVolume"></param>
        /// <param name="dayEntry"></param>
        private static void CompareDensity(double expectedDensity, DayEntry dayEntry)
        {
            if (Math.Round(dayEntry.density, 4) != Math.Round(expectedDensity, 4))
            {
                Assert.Fail("Density is not as expected. density from response:" + dayEntry.density +
                            " does not equal expected:" + expectedDensity);
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

        /// <summary>
        /// Converts an Olson time zone ID to a Windows time zone ID.
        /// </summary>
        /// <param name="olsonTimeZoneId">An Olson time zone ID. See http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/zone_tzid.html. </param>
        /// <returns>
        /// The TimeZoneInfo corresponding to the Olson time zone ID, 
        /// or null if you passed in an invalid Olson time zone ID.
        /// </returns>
        public static TimeZoneInfo OlsonTimeZoneToTimeZoneInfo(string olsonTimeZoneId)
        {
            var olsonWindowsTimes = new Dictionary<string, string>()
            {
                { "Africa/Bangui", "W. Central Africa Standard Time" },
                { "Africa/Cairo", "Egypt Standard Time" },
                { "Africa/Casablanca", "Morocco Standard Time" },
                { "Africa/Harare", "South Africa Standard Time" },
                { "Africa/Johannesburg", "South Africa Standard Time" },
                { "Africa/Lagos", "W. Central Africa Standard Time" },
                { "Africa/Monrovia", "Greenwich Standard Time" },
                { "Africa/Nairobi", "E. Africa Standard Time" },
                { "Africa/Windhoek", "Namibia Standard Time" },
                { "America/Anchorage", "Alaskan Standard Time" },
                { "America/Argentina/San_Juan", "Argentina Standard Time" },
                { "America/Asuncion", "Paraguay Standard Time" },
                { "America/Bahia", "Bahia Standard Time" },
                { "America/Bogota", "SA Pacific Standard Time" },
                { "America/Buenos_Aires", "Argentina Standard Time" },
                { "America/Caracas", "Venezuela Standard Time" },
                { "America/Cayenne", "SA Eastern Standard Time" },
                { "America/Chicago", "Central Standard Time" },
                { "America/Chihuahua", "Mountain Standard Time (Mexico)" },
                { "America/Cuiaba", "Central Brazilian Standard Time" },
                { "America/Denver", "Mountain Standard Time" },
                { "America/Fortaleza", "SA Eastern Standard Time" },
                { "America/Godthab", "Greenland Standard Time" },
                { "America/Guatemala", "Central America Standard Time" },
                { "America/Halifax", "Atlantic Standard Time" },
                { "America/Indianapolis", "US Eastern Standard Time" },
                { "America/La_Paz", "SA Western Standard Time" },
                { "America/Los_Angeles", "Pacific Standard Time" },
                { "America/Mexico_City", "Mexico Standard Time" },
                { "America/Montevideo", "Montevideo Standard Time" },
                { "America/New_York", "Eastern Standard Time" },
                { "America/Noronha", "UTC-02" },
                { "America/Phoenix", "US Mountain Standard Time" },
                { "America/Regina", "Canada Central Standard Time" },
                { "America/Santa_Isabel", "Pacific Standard Time (Mexico)" },
                { "America/Santiago", "Pacific SA Standard Time" },
                { "America/Sao_Paulo", "E. South America Standard Time" },
                { "America/St_Johns", "Newfoundland Standard Time" },
                { "America/Tijuana", "Pacific Standard Time" },
                { "Antarctica/McMurdo", "New Zealand Standard Time" },
                { "Atlantic/South_Georgia", "UTC-02" },
                { "Asia/Almaty", "Central Asia Standard Time" },
                { "Asia/Amman", "Jordan Standard Time" },
                { "Asia/Baghdad", "Arabic Standard Time" },
                { "Asia/Baku", "Azerbaijan Standard Time" },
                { "Asia/Bangkok", "SE Asia Standard Time" },
                { "Asia/Beirut", "Middle East Standard Time" },
                { "Asia/Calcutta", "India Standard Time" },
                { "Asia/Colombo", "Sri Lanka Standard Time" },
                { "Asia/Damascus", "Syria Standard Time" },
                { "Asia/Dhaka", "Bangladesh Standard Time" },
                { "Asia/Dubai", "Arabian Standard Time" },
                { "Asia/Irkutsk", "North Asia East Standard Time" },
                { "Asia/Jerusalem", "Israel Standard Time" },
                { "Asia/Kabul", "Afghanistan Standard Time" },
                { "Asia/Kamchatka", "Kamchatka Standard Time" },
                { "Asia/Karachi", "Pakistan Standard Time" },
                { "Asia/Katmandu", "Nepal Standard Time" },
                { "Asia/Kolkata", "India Standard Time" },
                { "Asia/Krasnoyarsk", "North Asia Standard Time" },
                { "Asia/Kuala_Lumpur", "Singapore Standard Time" },
                { "Asia/Kuwait", "Arab Standard Time" },
                { "Asia/Magadan", "Magadan Standard Time" },
                { "Asia/Muscat", "Arabian Standard Time" },
                { "Asia/Novosibirsk", "N. Central Asia Standard Time" },
                { "Asia/Oral", "West Asia Standard Time" },
                { "Asia/Rangoon", "Myanmar Standard Time" },
                { "Asia/Riyadh", "Arab Standard Time" },
                { "Asia/Seoul", "Korea Standard Time" },
                { "Asia/Shanghai", "China Standard Time" },
                { "Asia/Singapore", "Singapore Standard Time" },
                { "Asia/Taipei", "Taipei Standard Time" },
                { "Asia/Tashkent", "West Asia Standard Time" },
                { "Asia/Tbilisi", "Georgian Standard Time" },
                { "Asia/Tehran", "Iran Standard Time" },
                { "Asia/Tokyo", "Tokyo Standard Time" },
                { "Asia/Ulaanbaatar", "Ulaanbaatar Standard Time" },
                { "Asia/Vladivostok", "Vladivostok Standard Time" },
                { "Asia/Yakutsk", "Yakutsk Standard Time" },
                { "Asia/Yekaterinburg", "Ekaterinburg Standard Time" },
                { "Asia/Yerevan", "Armenian Standard Time" },
                { "Atlantic/Azores", "Azores Standard Time" },
                { "Atlantic/Cape_Verde", "Cape Verde Standard Time" },
                { "Atlantic/Reykjavik", "Greenwich Standard Time" },
                { "Australia/Adelaide", "Cen. Australia Standard Time" },
                { "Australia/Brisbane", "E. Australia Standard Time" },
                { "Australia/Darwin", "AUS Central Standard Time" },
                { "Australia/Hobart", "Tasmania Standard Time" },
                { "Australia/Perth", "W. Australia Standard Time" },
                { "Australia/Sydney", "AUS Eastern Standard Time" },
                { "Etc/GMT", "UTC" },
                { "Etc/GMT+11", "UTC-11" },
                { "Etc/GMT+12", "Dateline Standard Time" },
                { "Etc/GMT+2", "UTC-02" },
                { "Etc/GMT-12", "UTC+12" },
                { "Europe/Amsterdam", "W. Europe Standard Time" },
                { "Europe/Athens", "GTB Standard Time" },
                { "Europe/Belgrade", "Central Europe Standard Time" },
                { "Europe/Berlin", "W. Europe Standard Time" },
                { "Europe/Brussels", "Romance Standard Time" },
                { "Europe/Budapest", "Central Europe Standard Time" },
                { "Europe/Dublin", "GMT Standard Time" },
                { "Europe/Helsinki", "FLE Standard Time" },
                { "Europe/Istanbul", "GTB Standard Time" },
                { "Europe/Kiev", "FLE Standard Time" },
                { "Europe/London", "GMT Standard Time" },
                { "Europe/Minsk", "E. Europe Standard Time" },
                { "Europe/Moscow", "Russian Standard Time" },
                { "Europe/Paris", "Romance Standard Time" },
                { "Europe/Sarajevo", "Central European Standard Time" },
                { "Europe/Warsaw", "Central European Standard Time" },
                { "Indian/Mauritius", "Mauritius Standard Time" },
                { "Pacific/Apia", "Samoa Standard Time" },
                { "Pacific/Auckland", "New Zealand Standard Time" },
                { "Pacific/Fiji", "Fiji Standard Time" },
                { "Pacific/Guadalcanal", "Central Pacific Standard Time" },
                { "Pacific/Guam", "West Pacific Standard Time" },
                { "Pacific/Honolulu", "Hawaiian Standard Time" },
                { "Pacific/Pago_Pago", "UTC-11" },
                { "Pacific/Port_Moresby", "West Pacific Standard Time" },
                { "Pacific/Tongatapu", "Tonga Standard Time" }
            };

            var windowsTimeZoneId = default(string);
            var windowsTimeZone = default(TimeZoneInfo);
            if (olsonWindowsTimes.TryGetValue(olsonTimeZoneId, out windowsTimeZoneId))
            {
                try { windowsTimeZone = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return windowsTimeZone;
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
                if (allProjects.Any(prj => prj.id == projectId))
                    { return; }
                Assert.Fail("Project " + projectId + " does not exist list. Number of projects in list is " + allProjects.Count); 
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

        [When(@"adding a random weight for project \((.*)\) five days ago in timezone '(.*)'")]
        public void WhenAddingARandomWeightForProjectFiveDaysAgoInTimezone(int projectId, string timeZone)
        {
            SetAllDates(timeZone);
            AddAWeightToAProjectForADay(projectId, dateFiveDaysAgo);
        }

        [Then(@"check the random weight has been added to the project \((.*)\) for five days ago in timezone '(.*)'")]
        public void ThenCheckTheRandomWeightHasBeenAddedToTheProjectForFiveDaysAgoInTimezone(int projectId, string timeZone)
        {
            SetAllDates(timeZone);
            CheckTheWeightHasBeenAddedToTheProject(dateFiveDaysAgo);
        }


        [When(@"adding five random weights for project \((.*)\) ten days ago in timezone '(.*)'")]
        public void WhenAddingFiveRandomWeightsForProjectTenDaysAgoInTimezone(int projectId, string timeZone)
        {
            SetAllDates(timeZone);
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

        [Then(@"check the five random weights has been added each day to the project \((.*)\) in timezone '(.*)'")]
        public void ThenCheckTheFiveRandomWeightsHasBeenAddedEachDayToTheProjectInTimezone(int projectId, string timeZone)
        {
            SetAllDates(timeZone);
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


        [Then(@"check the density is re-calculated as \((.*)\) for the date \((.*)\)")]
        public void ThenCheckTheDensityIsCalculatedAsForTheDate(double expectedDensity, DateTime dateOfDensityCheck)
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
                CheckTheDayEntryIsValid(expectedDensity, dateOfDensityCheck, dayEntry.First<DayEntry>());
            }
        }

        // Expanding the tests to test ranges and negative tests
        [When(@"adding a random weight for project \((.*)\) yesterday in timezone '(.*)'")]
        public void WhenAddingARandomWeightForProjectYesterdayInTimezone(int projectId, string timeZone)
        {
            SetAllDates(timeZone);
            Log.Write("Adding a weight for date yesterday: " + dateYesterday.ToString());
            AddAWeightToAProjectForADay(projectId, dateYesterday);
        }


        [Then(@"check the random weight has been added to the project \((.*)\) for yesterday in timezone '(.*)'")]
        public void ThenCheckTheRandomWeightHasBeenAddedToTheProjectForYesterdayInTimezone(int projectId, string timeZone)
        {
            SetAllDates(timeZone);
            CheckTheWeightHasBeenAddedToTheProject(dateYesterday);
        }

        [When(@"adding a random weight for project \((.*)\) today in timezone '(.*)'")]
        public void WhenAddingARandomWeightForProjectTodayInTimezone(int projectId, string timeZone)
        {
            SetAllDates(timeZone);
            Log.Write("Adding a weight for today: " + dateToday.ToString());
            AddAWeightToAProjectForADay(projectId, dateToday);
        }

        [When(@"adding a random weight for project \((.*)\) tomorrow in timezone '(.*)'")]
        public void WhenAddingARandomWeightForProjectTomorrowInTimezone(int projectId, string timeZone)
        {
            SetAllDates(timeZone);
            Log.Write("Adding a weight for date tomorrow: " + dateTomorrow.ToString());
            AddAWeightToAProjectForADay(projectId, dateTomorrow);
        }

        #endregion
    }
}
