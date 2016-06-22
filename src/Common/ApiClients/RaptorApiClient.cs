using System.Globalization;
using LandfillService.Common.Context;
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
        private string reportEndpoint;
        private string prodDataEndpoint;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RaptorApiClient()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RaptorApiUrl"] ?? "/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(60 * 60);
            this.reportEndpoint = ConfigurationManager.AppSettings["RaptorReportEndpoint"];
            this.prodDataEndpoint = ConfigurationManager.AppSettings["RaptorProdDataEndpoint"];
        }

        public void Dispose()
        {
            client.Dispose();
        }

        /// <summary>
        /// Makes a JSON GET or POST request to the Raptor API
        /// </summary>
        /// <param name="endpoint">URL path fragment for the request</param>
        /// <param name="method">Method of request (GET or POST)</param>
        /// <param name="userUid">User ID</param>
        /// <param name="content">Request content</param>
        /// <returns>Response as a string; throws an exception if the request is not successful</returns>
        private async Task<string> Request(string endpoint, HttpMethod method, string userUid, HttpContent content)  
        {
            Log.DebugFormat("In RaptorApiClient::Request to " + endpoint + " with " + content);

            LoggerSvc.LogRequest(GetType().Name, MethodBase.GetCurrentMethod().Name, client.BaseAddress + endpoint, content);

            HttpRequestMessage request = new HttpRequestMessage(method, endpoint);
            request.Headers.Add("Authorization", "VL " + userUid);
            request.Content = content;
            
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
        /// Makes a JSON POST request to the Raptor API
        /// </summary>
        /// <param name="endpoint">URL path fragment for the request</param>
        /// <param name="userUid">User ID</param>
        /// <param name="parameters">JSON parameters</param>
        /// <returns>Response as a string; throws an exception if the request is not successful</returns>
        private async Task<string> Request<TParams>(string endpoint, string userUid, TParams parameters)
        {
         var content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
          return await Request(endpoint, HttpMethod.Post, userUid, content);
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
        private async Task<SummaryVolumesResult> GetVolumesAsync(string userUid, Project project, DateTime date, List<WGSPoint> geofence)
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
          return ParseResponse<SummaryVolumesResult>(await Request(this.reportEndpoint + "volumes/summary", userUid, volumeParams));
        }

        /// <summary>
        /// Retrieves volume summary from Raptor and saves it to the landfill DB
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">Project</param>
        /// <param name="geofence">Geofence</param>
        /// <param name="entry">Weight entry from the client</param>
        /// <returns></returns>
        public async Task GetVolumeInBackground(string userUid, Project project, List<WGSPoint> geofence, DateEntry entry)
        {
          try
          {
            Log.DebugFormat("Get volume for project {0} date {1}", project.id, entry.date);

            var res = await GetVolumesAsync(userUid, project, entry.date, geofence);

            Log.Debug("Volume res:" + res);
            Log.Debug("Volume: " + (res.Fill));

            LandfillDb.SaveVolume(project.projectUid, entry.geofenceUid, entry.date, res.Fill);
          }
          catch (RaptorApiException e)
          {
            if (e.code == HttpStatusCode.BadRequest)
            {
              // this response code is returned when the volume isn't available (e.g. the time range
              // is outside project extents); the assumption is that's the only reason we will
              // receive a 400 Bad Request 

              Log.Warn("RaptorApiException while retrieving volumes: " + e.Message);
              LandfillDb.MarkVolumeNotAvailable(project.projectUid, entry.geofenceUid, entry.date);

              // TESTING CODE
              // Volume range in m3 should be ~ [478, 1020]
              //LandfillDb.SaveVolume(project.id, entry.date, new Random().Next(541) + 478, entry.geofenceUid);
            }
            else
            {
              Log.Error("RaptorApiException while retrieving volumes: " + e.Message);
              LandfillDb.MarkVolumeNotRetrieved(project.projectUid, entry.geofenceUid, entry.date);
            }
          }
          catch (Exception e)
          {
            Log.Error("Exception while retrieving volumes: " + e.Message);
            LandfillDb.MarkVolumeNotRetrieved(project.projectUid, entry.geofenceUid, entry.date);
          }
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
          Log.DebugFormat("UTC time range in CCA request: {0} - {1}", utcDateTime, utcDateTime.AddDays(1));

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
        private async Task<CCASummaryResult> GetCCAAsync(string userUid, Project project, DateTime date, MachineDetails machine, int? liftId, List<WGSPoint> geofence)
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
          return ParseResponse<CCASummaryResult>(await Request(this.reportEndpoint + "compaction/cca/summary", userUid, ccaParams));
        }

        /// <summary>
        /// Retrieves CCA summary from Raptor and saves it to the landfill DB
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">Project</param>
        /// <param name="geofence">Geofence boundary</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date to retrieve CCA for (in project time zone)</param>
        /// <param name="machineId">Landfill Machine ID</param>
        /// <param name="machine">Machine details</param>
        /// <param name="liftId">Lift/layer number. If not specified then CCA retrieved for all lifts</param>
        /// <returns></returns>
        public async Task GetCCAInBackground(string userUid, Project project, string geofenceUid, List<WGSPoint> geofence, DateTime date, long machineId, MachineDetails machine, int? liftId)
        {
          try
          {
            Log.DebugFormat("Get CCA for project {0} date {1} machine {2}", project.id, date, machine.machineName);

            var res = await GetCCAAsync(userUid, project, date, machine, liftId, geofence);

            Log.Debug("CCA res:" + res);
            Log.DebugFormat("CCA: incomplete {0}, complete {1}, overcomplete {2}", res.incompletePercent, res.completePercent, res.overcompletePercent);

            LandfillDb.SaveCCA(project.projectUid, geofenceUid, date, machineId, liftId, res.incompletePercent, res.completePercent, res.overcompletePercent);
          }
          catch (RaptorApiException e)
          {
            if (e.code == HttpStatusCode.BadRequest)
            {
              // this response code is returned when the CCA isn't available (e.g. the time range
              // is outside project extents); the assumption is that's the only reason we will
              // receive a 400 Bad Request 

              Log.Warn("RaptorApiException while retrieving CCA: " + e.Message);
              LandfillDb.MarkCCANotAvailable(project.projectUid, geofenceUid, date, machineId, liftId);
            }
            else
            {
              Log.Error("RaptorApiException while retrieving CCA: " + e.Message);
              LandfillDb.MarkCCANotRetrieved(project.projectUid, geofenceUid, date, machineId, liftId);
            }
          }
          catch (Exception e)
          {
            Log.Error("Exception while retrieving CCA: " + e.Message);
            LandfillDb.MarkCCANotRetrieved(project.projectUid, geofenceUid, date, machineId, liftId);
          }
        }

      /// <summary>
      /// Retrieves a list of machines and lifts for the project for the given date range
      /// </summary>
      /// <param name="userUid">User ID</param>
      /// <param name="project">Project</param>
      /// <param name="startUtc">UTC start date</param>
      /// <param name="endUtc">UTC end date</param>
      /// <returns></returns>
      private async Task<MachineLiftDetails[]> GetMachineLiftListAsync(string userUid, Project project, DateTime startUtc, DateTime endUtc)
      {        
          string url = string.Format("{0}projects/{1}/machinelifts?startUtc={2}&endUtc={3}",
              this.prodDataEndpoint, project.id, FormatUtcDate(startUtc), FormatUtcDate(endUtc));
          return ParseResponse<MachineLiftDetails[]>(Request(url, HttpMethod.Get, userUid, null).Result);
      }

      /// <summary>
      /// Retrieves a list of machines and lifts for the project for the given date range
      /// </summary>
      /// <param name="userUid">User ID</param>
      /// <param name="project">Project</param>
      /// <param name="startUtc">UTC start date</param>
      /// <param name="endUtc">UTC end date</param>
      /// <returns></returns>
      public MachineLiftDetails[] GetMachineLiftList(string userUid, Project project, DateTime startUtc, DateTime endUtc)
      {
        //TODO: We should retry for 400 error - means Raptor is down
        try
        {
          var list = GetMachineLiftListAsync(userUid, project, startUtc, endUtc).Result;
          return list;
        }
        catch (RaptorApiException e)
        {
          if (e.code == HttpStatusCode.BadRequest)
          {
            Log.Warn("RaptorApiException while retrieving machines & lifts: " + e.Message);
          }
        }
        catch (Exception e)
        {
          Log.Error("Exception while retrieving machines & lifts: " + e.Message);
        }
        return new MachineLiftDetails[0];
      }

      /// <summary>
      /// Formats UTC date in ISO 8601 format for Raptor Services Web API.
      /// </summary>
      /// <param name="utcDate">The UTC date to format</param>
      /// <returns>ISO 8601 formatted date</returns>
      private string FormatUtcDate(DateTime utcDate)
      {
        var dateUtc = new DateTime(utcDate.Ticks, DateTimeKind.Utc);
        var utcStr = dateUtc.ToString("o", CultureInfo.InvariantCulture);
        //Remove the trailing millisecs
        return string.Format("{0}Z", utcStr.Remove(utcStr.IndexOf(".")));
      }
    }
}