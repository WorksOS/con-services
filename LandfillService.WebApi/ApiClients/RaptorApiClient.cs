using LandfillService.Common;
using LandfillService.WebApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace LandfillService.WebApi.ApiClients
{
    /// <summary>
    /// This exception can be thrown when the Raptor API returns an unsuccessful response (which needs to be propagated to the client)
    /// </summary>
    public class RaptorApiException : ApplicationException
    {
        public HttpStatusCode code { get; set; }

        public RaptorApiException(HttpStatusCode c, string message) : base(message)
        {
            code = c;
        }
    }

    /// <summary>
    /// A wrapper around HttpClient to handle requests to the Raptor API
    /// </summary>
    public class RaptorApiClient : IDisposable
    {
        private HttpClient client;

        public RaptorApiClient()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RaptorApiUrl"] ?? "/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(60 * 60);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        /// <summary>
        /// Makes a JSON POST request to the Foreman API
        /// </summary>
        /// <param name="endpoint">URL path fragment for the request</param>
        /// <param name="sessionId">session ID provided by the Foreman API</param>
        /// <param name="parameters">JSON parameters</param>
        /// <returns>Response as a string; throws an exception if the request is not successful</returns>
        private async Task<string> Request<TParams>(string endpoint, string sessionId, TParams parameters)  
        {
            System.Diagnostics.Debug.WriteLine("In RaptorApiClient::Request to " + endpoint + " with " + parameters);

            LoggerSvc.LogRequest(GetType().Name, MethodBase.GetCurrentMethod().Name, client.BaseAddress + endpoint, parameters);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", "VL " + sessionId);
            request.Content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");

            
            // Syncronous processing can be forced by calling .Result
            var response = await client.SendAsync(request); 

            if (!response.IsSuccessStatusCode)
            {
                LoggerSvc.LogResponse(GetType().Name, MethodBase.GetCurrentMethod().Name, client.BaseAddress + endpoint, response);
                throw new RaptorApiException(response.StatusCode, response.ReasonPhrase);
            }

            System.Diagnostics.Debug.WriteLine("POST request succeeded");
            LoggerSvc.LogResponse(GetType().Name, MethodBase.GetCurrentMethod().Name, client.BaseAddress + endpoint, response);

            var responseContent = response.Content;

            var res = await responseContent.ReadAsStringAsync();
            return res;
        }

        /// <summary>
        /// Parses a JSON response from the Raptor API
        /// </summary>
        /// <param name="response">response string</param>
        /// <returns>Response string converted to the given type</returns>
        private T ParseResponse<T>(string response)
        {
            System.Diagnostics.Debug.WriteLine("In RaptorApiClient::ParseResponse");

            var resObj = JsonConvert.DeserializeObject<T>(response,
                 new JsonSerializerSettings
                 {
                     Error = delegate(object sender, ErrorEventArgs args)
                     {
                         System.Diagnostics.Debug.WriteLine(args.ErrorContext.Error.Message);
                         args.ErrorContext.Handled = true;
                     }
                 });

            return resObj;
        }


        /// <summary>
        /// Retrieves volume summary information for a given project and date
        /// </summary>
        /// <param name="sessionId">session ID provided by the Foreman API</param>
        /// <param name="project">VisionLink project to retrieve volumes for</param>
        /// <param name="date">Date to retrieve volumes for (in project time zone)</param>
        /// <returns>Response as a string; throws an exception if the request is not successful</returns>
        public async Task<SummaryVolumesResult> GetVolumesAsync(string sessionId, Project project, DateTime date)
        {
            var projTimeZone = DateTimeZoneProviders.Tzdb[project.timeZoneName];
            var dateInProjTimeZone = projTimeZone.AtLeniently(new LocalDateTime(date.Year, date.Month, date.Day, 0, 0));
            var utcDateTime = dateInProjTimeZone.ToDateTimeUtc();

            System.Diagnostics.Debug.WriteLine("UTC time range in volume request: {0} - {1}", utcDateTime.ToString(), utcDateTime.AddDays(1).ToString());

            var volumeParams = new VolumeParams()
            {
                projectId = project.id,
                volumeCalcType = 4,
                baseFilter = new VolumeFilter() { startUTC = utcDateTime, endUTC = utcDateTime.AddDays(1), returnEarliest = true },
                topFilter = new VolumeFilter() { startUTC = utcDateTime, endUTC = utcDateTime.AddDays(1), returnEarliest = false }
            };
            return ParseResponse<SummaryVolumesResult>(await Request("volumes/summary", sessionId, volumeParams));
        }

    }
}