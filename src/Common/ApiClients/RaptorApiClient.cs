using LandfillService.Common.Models;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NodaTime.TimeZones;

namespace LandfillService.Common.ApiClients
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

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
        /// Makes a JSON POST request to the Raptor API
        /// </summary>
        /// <param name="endpoint">URL path fragment for the request</param>
        /// <param name="userUid">User ID</param>
        /// <param name="parameters">JSON parameters</param>
        /// <returns>Response as a string; throws an exception if the request is not successful</returns>
        private async Task<string> Request<TParams>(string endpoint, string userUid, TParams parameters)  
        {
            Log.DebugFormat("In RaptorApiClient::Request to " + endpoint + " with " + parameters);

            LoggerSvc.LogRequest(GetType().Name, MethodBase.GetCurrentMethod().Name, client.BaseAddress + endpoint, parameters);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", "VL " + userUid);
            request.Content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");

            
            // Syncronous processing can be forced by calling .Result
            var response = await client.SendAsync(request); 

            if (!response.IsSuccessStatusCode)
            {
                LoggerSvc.LogResponse(GetType().Name, MethodBase.GetCurrentMethod().Name, client.BaseAddress + endpoint, response);
                Log.WarnFormat("Bad response from Raptor {0}", response);
                throw new RaptorApiException(response.StatusCode, response.ReasonPhrase);
            }

            Log.DebugFormat("POST request succeeded");
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
          Log.DebugFormat("In RaptorApiClient::ParseResponse");

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
        /// Retrieves volume summary information for a given project, date and geofence
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">VisionLink project to retrieve volumes for</param>
        /// <param name="date">Date to retrieve volumes for (in project time zone)</param>
        /// <param name="geofence">Geofence to retrieve volumes for. If not specified then volume retrieved for entire project area</param>
        /// <returns>Response as a string; throws an exception if the request is not successful</returns>
        public async Task<SummaryVolumesResult> GetVolumesAsync(string userUid, Project project, DateTime date, List<WGSPoint> geofence)
        {
          DateTime startUtc;
          DateTime endUtc;
          ConvertToUtc(date, project.timeZoneName, out startUtc, out endUtc);

          var volumeParams = new VolumeParams
          {
            projectId = project.id,
            volumeCalcType = 4,
            baseFilter = new VolumeFilter
            {
              startUTC = startUtc,
              endUTC = endUtc,
              returnEarliest = true,
              polygonLL = geofence
            },
            topFilter = new VolumeFilter
            {
              startUTC = startUtc,
              endUTC = endUtc,
              returnEarliest = false,
              polygonLL = geofence
            }
          };
          return ParseResponse<SummaryVolumesResult>(await Request("volumes/summary", userUid, volumeParams));
        }


        private TimeZoneInfo GetTimeZoneInfoForTzdbId(string tzdbId)
        {
          var mappings = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones;
          var map = mappings.FirstOrDefault(x =>
              x.TzdbIds.Any(z => z.Equals(tzdbId, StringComparison.OrdinalIgnoreCase)));
          return map == null ? null : TimeZoneInfo.FindSystemTimeZoneById(map.WindowsId);
        }

        private void ConvertToUtc(DateTime date, string timeZoneName, out DateTime startUtc, out DateTime endUtc)
        {
          TimeZoneInfo hwZone = GetTimeZoneInfoForTzdbId(timeZoneName);

          //use only utc dates and times in the service contracts. Ignore time for now.
          var utcDateTime = date.Date.Add(-hwZone.BaseUtcOffset);
          Log.DebugFormat("UTC time range in CCA request: {0} - {1}", utcDateTime.ToString(), utcDateTime.AddDays(1).ToString());

          startUtc = utcDateTime;
          endUtc = utcDateTime.AddDays(1).AddMinutes(-1);
        }

        /// <summary>
        /// Retrieves CCA summary information for a given project, date and machine
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">VisionLink project to retrieve volumes for</param>
        /// <param name="date">Date to retrieve CCA for (in project time zone)</param>
        /// <param name="machine">Machine to retrieve CCA for</param>
        /// <param name="geofence">Geofence to retrieve CCA for. If not specified then CCA retrieved for entire project area</param>
        /// <param name="liftId">Lift/layer number to retrieve CCA for. If not specified then CCA retrieved for all lifts</param>
        /// <returns>Response as a string; throws an exception if the request is not successful</returns>
        public async Task<SummaryVolumesResult> GetCCAAsync(string userUid, Project project, DateTime date, MachineDetails machine, int? liftId, List<WGSPoint> geofence)
        {
          DateTime startUtc;
          DateTime endUtc;
          ConvertToUtc(date, project.timeZoneName, out startUtc, out endUtc);

          var ccaParams = new CCASummaryParams
          {
            projectId = project.id,
            filter = new CCAFilter
            {
              startUTC = startUtc,
              endUTC = endUtc,
              contributingMachines = new List<MachineDetails>{machine},
              layerNumber = liftId,
              polygonLL = geofence
            },         
          };
          return ParseResponse<SummaryVolumesResult>(await Request("compaction/cca/summary", userUid, ccaParams));
        }

    }
}