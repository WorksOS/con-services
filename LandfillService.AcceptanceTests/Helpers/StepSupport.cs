using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LandfillService.WebApi.Models;

namespace LandfillService.AcceptanceTests.Helpers
{
    public class StepSupport
    {
        private static readonly Random rndNumber = new Random();
        private static readonly object syncLock = new object();

        /// <summary>
        /// Get the unit type from response from forman. 
        /// </summary>
        /// <param name="inResponse"></param>
        public UnitsTypeEnum GetWeightUnitsFromResponse(string inResponse)
        {
            if (inResponse.Length < 32)
               { return UnitsTypeEnum.Metric; }

            var unitsSetting = inResponse.Substring(33).TrimEnd();
            switch (unitsSetting)
            {
                case "Imperial":
                    return UnitsTypeEnum.Imperial;
                case "Metric":
                    return UnitsTypeEnum.Metric;
                case "US":
                    return UnitsTypeEnum.US;
                default:
                    return UnitsTypeEnum.Metric;
            }
        }

        /// <summary>
        /// Get the session ID from response from forman. 
        /// </summary>
        /// <param name="inResponse"></param>
        public string GetSessionIdFromResponse(string inResponse)
        {
            if (inResponse.Length < 32)
                { return inResponse; }
            return inResponse.Substring(0, 32); 
        }

        /// <summary>
        /// Get a random weight in tonnes 
        /// </summary>
        /// <returns>A double between 2000 and 3500</returns>
        public double GetRandomWeight()
        {
            Random random = new Random();
            var randomWeight = random.Next(2000, 3500);
            return randomWeight;
        }

        /// <summary>
        /// Set up the weight for one day
        /// </summary>
        /// <param name="oneDayDate">The date you want the weight set up for</param>
        /// <param name="weight">Random weight for the day</param>
        /// <returns>One Weightentry</returns>
        public WeightEntry[] SetUpOneWeightForOneDay(DateTime oneDayDate, double weight)
        {
            WeightEntry[] weightForOneDay = { 
                new WeightEntry {date = oneDayDate.Date, weight = weight}
            };
            return weightForOneDay;
        }

        /// <summary>
        /// Set up the wait for one day with a random weight
        /// </summary>
        /// <param name="oneDayDate">A valid date</param>
        /// <returns>One random weight entry</returns>
        public WeightEntry[] SetUpOneWeightForOneDay(DateTime oneDayDate)
        {
            var randomWeight = GetRandomWeight();
            WeightEntry[] weightForOneDay = { 
                new WeightEntry {date = oneDayDate.Date, weight = randomWeight}
            };
            return weightForOneDay;
        }



        /// <summary>
        /// Sets up 5 weights that are loaded up to web service
        /// </summary>
        /// <returns>Five entries of in array of weight entry</returns>
        public WeightEntry[] SetUpFiveWeightsForUpload(DateTime dateToday)
        {
            var randomWeight = GetRandomWeight();
            WeightEntry[] weightForFiveDays = { 
                new WeightEntry {date = dateToday.AddDays(-11).Date, weight = randomWeight},
                new WeightEntry {date = dateToday.AddDays(-10).Date, weight = randomWeight}, 
                new WeightEntry {date = dateToday.AddDays(-9).Date, weight = randomWeight}, 
                new WeightEntry {date = dateToday.AddDays(-8).Date, weight = randomWeight}, 
                new WeightEntry {date = dateToday.AddDays(-7).Date, weight = randomWeight} 
            };
            return weightForFiveDays;
        }

        /// <summary>
        /// Set up all the dates used in the tests.
        /// </summary>
        /// <summary>
        /// Get five days ago for the time zone 
        /// </summary>
        /// <param name="projectTimeZone">
        /// </param>
        public DateTime GetFiveDaysAgoForTimeZone(string projectTimeZone)
        {
            TimeZoneInfo hwZone = OlsonTimeZoneToTimeZoneInfo(projectTimeZone);
            var fiveDaysOldForProjectTimeZone = DateTime.UtcNow.Add(hwZone.BaseUtcOffset);
            return fiveDaysOldForProjectTimeZone.AddDays(-5);
        }

        /// <summary>
        /// Get Yesterday for the time zone 
        /// </summary>
        /// <param name="projectTimeZone"></param>
        public DateTime GetYesterdayForTimeZone(string projectTimeZone)
        {
            TimeZoneInfo hwZone = OlsonTimeZoneToTimeZoneInfo(projectTimeZone);
            var yesterdayForProjectTimeZone = DateTime.UtcNow.Add(hwZone.BaseUtcOffset);
            return yesterdayForProjectTimeZone.AddDays(-1);
        }

        /// <summary>
        /// Get today for the time zone 
        /// </summary>
        /// <param name="projectTimeZone"></param>
        public DateTime GetTodayForTimeZone(string projectTimeZone)
        {
            TimeZoneInfo hwZone = OlsonTimeZoneToTimeZoneInfo(projectTimeZone);
            var todayForProjectTimeZone = DateTime.UtcNow.Add(hwZone.BaseUtcOffset);
            return todayForProjectTimeZone;
        }

        /// <summary>
        /// Get tomorrow for the time zone 
        /// </summary>
        /// <param name="projectTimeZone"></param>
        public DateTime GetTomorrowForTimeZone(string projectTimeZone)
        {
            TimeZoneInfo hwZone = OlsonTimeZoneToTimeZoneInfo(projectTimeZone);
            var tomorrowForProjectTimeZone = DateTime.UtcNow.Add(hwZone.BaseUtcOffset);
            return tomorrowForProjectTimeZone.AddDays(1);
        }

        /// <summary>
        /// Converts an Olson time zone ID to a Windows time zone ID.
        /// </summary>
        /// <param name="olsonTimeZoneId"/>An Olson time zone ID./param>
        /// <returns>The TimeZoneInfo corresponding to the Olson time zone ID, or null if you passed in an invalid Olson time zone ID. </returns>
        public TimeZoneInfo OlsonTimeZoneToTimeZoneInfo(string olsonTimeZoneId)
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

            string windowsTimeZoneId;
            var windowsTimeZone = default(TimeZoneInfo);
            if (olsonWindowsTimes.TryGetValue(olsonTimeZoneId, out windowsTimeZoneId))
            {
                try { windowsTimeZone = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return windowsTimeZone;
        }

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            {
                return rndNumber.Next(min, max);
            }
        }

        /// <summary>
        /// Build a random number using date's and random generator. This is used aas the asset ID on the 
        /// </summary>
        /// <returns>16 character random string prefixed</returns>
        public string GetRandomNumber()
        {
            var rnd = RandomNumber(1, 987654321);
            var unique = DateTime.Now.Year + DateTime.Now.DayOfYear +
                        GetNumbersOnly(DateTime.Now.TimeOfDay + DateTime.Now.Millisecond.ToString()) +
                        rnd;
            return unique.Length <= 16 ? unique : unique.Substring(0, 16);
        }

        /// <summary>
        /// Get only the numbers
        /// </summary>
        /// <param name="input">String of text with numbers</param>
        /// <returns>The numbers only</returns>
        private static string GetNumbersOnly(string input)
        {
            return new string(input.Where(char.IsDigit).ToArray());
        }

    }
}
