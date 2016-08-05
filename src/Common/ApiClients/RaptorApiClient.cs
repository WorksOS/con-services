using System.Globalization;
using System.Web.Http.Controllers;
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

            Log.DebugFormat("{0} request succeeded", method);
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
          Log.DebugFormat("Parsing {0} into {1}",response,typeof(T));


            var resObj = JsonConvert.DeserializeObject<T>(response,
                 new JsonSerializerSettings
                 {
                     Error = delegate(object sender, ErrorEventArgs args)
                     {
                       Log.DebugFormat("Error {0}", args.ErrorContext.Error.Message);
                       args.ErrorContext.Handled = true;
                     },
                     MissingMemberHandling = MissingMemberHandling.Ignore
                 });
            Log.DebugFormat("Reversed serialization {0}", JsonConvert.SerializeObject(resObj));
            return resObj;
        }


        /// <summary>
        /// Retrieves volume summary information for a given project, date and geofence
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">VisionLink project to retrieve volumes for</param>
        /// <param name="date">Date to retrieve volumes for (in project time zone)</param>
        /// <param name="geofence">Geofence to retrieve volumes for. If not specified then volume retrieved for entire project area</param>
        /// <returns>Summary volumes</returns>
        private async Task<SummaryVolumesResult> GetVolumesAsync(string userUid, Project project, DateTime date, List<WGSPoint> geofence)
        {
          DateTime startUtc;
          DateTime endUtc;
          ConvertToUtc(date, project.timeZoneName, out startUtc, out endUtc);
          Log.DebugFormat("UTC time range in Volume request: {0} - {1}", startUtc, endUtc);

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

        /// <summary>
        /// Retrieves airspace volume summary information for a given project and date. This is the volume remaining
        /// for the project calculated as the volume between the current ground surface and the design surface.
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">VisionLink project to retrieve volumes for</param>
        /// <param name="date">Date to retrieve volumes for (in project time zone)</param>
        /// <param name="returnEarliest">Flag to indicate if earliest or latest cell pass to be used</param>
        /// <returns>SummaryVolumesResult</returns>
        public async Task<SummaryVolumesResult> GetAirspaceVolumeAsync(string userUid, Project project, bool returnEarliest)
        {
          string tccFilespaceId = ConfigurationManager.AppSettings["TCCfilespaceId"];
          string topOfWasteDesignFilename = ConfigurationManager.AppSettings["TopOfWasteDesignFilename"];
          var volumeParams = new VolumeParams
          {
            projectId = project.id,
            volumeCalcType = 5,
            baseFilter = new VolumeFilter { returnEarliest = returnEarliest },
            topDesignDescriptor = new VolumeDesign
            {
              file = new DesignDescriptor { filespaceId = tccFilespaceId, path = String.Format("/{0}/{1}", project.legacyCustomerID, project.id), fileName = topOfWasteDesignFilename }
            }
          };
          return ParseResponse<SummaryVolumesResult>(await Request(this.reportEndpoint + "volumes/summary", userUid, volumeParams));
        }

        /// <summary>
        /// Retrieves project statistics information for a given project. 
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">VisionLink project to retrieve volumes for</param>
        /// <returns>ProjectStatisticsResult</returns>
        public async Task<ProjectStatisticsResult> GetProjectStatisticsAsync(string userUid, Project project)
        {
          var statsParams = new StatisticsParams { projectId = project.id };
          return ParseResponse<ProjectStatisticsResult>(await Request(this.reportEndpoint + "projects/statistics", userUid, statsParams));
        }


        public TimeZoneInfo GetTimeZoneInfoForTzdbId(string tzdbId)
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
        /// <returns>CCASummaryResult</returns>
        private async Task<CCASummaryResult> GetCCAAsync(string userUid, Project project, DateTime date, MachineDetails machine, int? liftId, List<WGSPoint> geofence)
        {
          DateTime startUtc;
          DateTime endUtc;
          ConvertToUtc(date, project.timeZoneName, out startUtc, out endUtc);
          Log.DebugFormat("UTC time range in CCA request: {0} - {1}", startUtc, endUtc);

          //This is because we sometimes pass MachineLiftDetails and the serialization
          //will do the derived class and RaptorServices complains about the extra properties.
          MachineDetails details = new MachineDetails
                                   {
                                       assetId = machine.assetId,
                                       machineName = machine.machineName,
                                       isJohnDoe = machine.isJohnDoe
                                   };
           
          var ccaParams = new CCASummaryParams
          {
            projectId = project.id,
            filter = new CCAFilter
            {
              startUTC = startUtc,
              endUTC = endUtc,
              contributingMachines = new List<MachineDetails>{details},
              layerNumber = liftId,
              layerType = liftId.HasValue ? 7 : (int?)null, //7 = TagFile!
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
            Log.DebugFormat("Get CCA for projectId {0} date {1} machine name {2} machine id {3} geofenceUid {4} liftId {5}", 
              project.id, date, machine.machineName, machineId, geofenceUid, liftId);

            var res = await GetCCAAsync(userUid, project, date, machine, liftId, geofence);

            Log.Debug("CCA res:" + res);
            Log.DebugFormat("CCA: incomplete {0}, complete {1}, overcomplete {2}", res.undercompletePercent, res.completePercent, res.overcompletePercent);

            LandfillDb.SaveCCA(project.projectUid, geofenceUid, date, machineId, liftId, res.undercompletePercent, res.completePercent, res.overcompletePercent);
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
      /// Retrieves a list of machines and lifts for the project for the given datetime range.
      /// </summary>
      /// <param name="userUid">User ID</param>
      /// <param name="project">Project to retrieve machines and lifts for</param>
        /// <param name="startUtc">Start UTC to retrieve machines and lifts for</param>
        /// <param name="endUtc">End UTC to retrieve machines and lifts for</param>
        /// <returns>Machines and lifts</returns>
      private async Task<MachineLayerIdsExecutionResult> GetMachineLiftListAsync(string userUid, Project project, DateTime startUtc, DateTime endUtc)
      {
        string url = string.Format("{0}projects/{1}/machinelifts?startUtc={2}&endUtc={3}",
            this.prodDataEndpoint, project.id, FormatUtcDate(startUtc), FormatUtcDate(endUtc));
        return ParseResponse<MachineLayerIdsExecutionResult>(await Request(url, HttpMethod.Get, userUid, null));
      }

      /// <summary>
      /// Retrieves a list of machines and lifts for the project for the given date range.
      /// </summary>
      /// <param name="userUid">User ID</param>
      /// <param name="project">Project</param>
      /// <param name="startDate">Start date in project time zone</param>
      /// <param name="endDate">End date in project time zone</param>
      /// <returns>List of machines and lifts in project time zone</returns>
      public async Task<List<MachineLifts>> GetMachineLiftsInBackground(string userUid, Project project, DateTime startDate, DateTime endDate)
      {
        try
        {
          DateTime startUtc1;
          DateTime endUtc1;
          ConvertToUtc(startDate, project.timeZoneName, out startUtc1, out endUtc1);
          DateTime startUtc2;
          DateTime endUtc2;
          ConvertToUtc(endDate, project.timeZoneName, out startUtc2, out endUtc2);
          var result = await GetMachineLiftListAsync(userUid, project, startUtc1, endUtc2);
          return GetMachineLiftsInProjectTimeZone(project, endUtc2, result.MachineLiftDetails.ToList());
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
        return new List<MachineLifts>();
      }

      /// <summary>
      /// Converts the list of machines and lifts from Raptor to the list for the Web API. 
      /// Raptor can have multiple entries per day for a lift whereas the Web API only wants one.
      /// Also Raptor uses UTC while the Web API uses the project time zone.
      /// Finally Raptor lifts can continue past the end of the day while the Web API wants to stop at the end of the day.
      /// </summary>
      /// <param name="project">The project for which the machine/lifts conversion is occurring.</param>
      /// <param name="endUtc">The start UTC for the machine/lifts</param>
      /// <param name="machineList">The list of machines and lifts returned by Raptor</param>
      /// <returns>List of machines and lifts in project time zone.</returns>
      private List<MachineLifts> GetMachineLiftsInProjectTimeZone(Project project, DateTime endUtc, IEnumerable<MachineLiftDetails> machineList)
      {
        TimeZoneInfo hwZone = GetTimeZoneInfoForTzdbId(project.timeZoneName);

        List<MachineLifts> machineLifts = new List<MachineLifts>();
        foreach (var machine in machineList)
        {
          var machineLift = new MachineLifts {assetId = machine.assetId, machineName = machine.machineName, isJohnDoe = machine.isJohnDoe};
          //Only want last lift of the day for each lift
          var orderedLifts = machine.lifts.OrderBy(l => l.layerId).ThenByDescending(l => l.endUtc);
          List<Lift> lifts = new List<Lift>();
          foreach (var orderedLift in orderedLifts)
          {
            if (lifts.Where(l => l.layerId == orderedLift.layerId).FirstOrDefault() == null)
            {
              //If the lift is still active at the end of the day the use the end of the day
              if (orderedLift.endUtc > endUtc)
                orderedLift.endUtc = endUtc;
              lifts.Add(new Lift { layerId = orderedLift.layerId, endTime = orderedLift.endUtc.Add(hwZone.BaseUtcOffset) });            
            }
          }
          machineLift.lifts = lifts;
          machineLifts.Add(machineLift);
        }
        return machineLifts;
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