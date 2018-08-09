using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Repository;
using LandfillService.Common.ApiClients;
using LandfillService.Common.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.TimeZones;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;

namespace Common.netstandard.ApiClients
{
  /// <summary>
  ///   This exception can be thrown when the Raptor API returns an unsuccessful response (which needs to be propagated to
  ///   the client)
  /// </summary>
  public class RaptorApiException : ApplicationException
  {
    public RaptorApiException(HttpStatusCode c, string message) : base(message)
    {
      code = c;
    }

    public HttpStatusCode code { get; set; }
  }

  /// <summary>
  ///   A wrapper around HttpClient to handle requests to the Raptor API
  /// </summary>
  public class RaptorApiClient :  IRaptorApiClient
  {
    private readonly IConfigurationStore config;
    private readonly ILogger Log;
    //private readonly string prodDataEndpoint;
    private IRaptorProxy raptorProxy;
    //private readonly string reportEndpoint;
    //private string baseAddress;
    public IDictionary<string, string> customHeaders;
    private IFileListProxy filesProxy;

    public RaptorApiClient(ILogger Log, IConfigurationStore config, IRaptorProxy proxy, IFileListProxy projectProxy, IDictionary<string, string> customHeaders)
    {
      //baseAddress = config.GetValueString("RaptorApiUrl") ?? "/";
      //reportEndpoint = config.GetValueString("RaptorReportEndpoint");
      //prodDataEndpoint = config.GetValueString("RaptorProdDataEndpoint");
      this.Log = Log;
      this.config = config;
      raptorProxy = proxy;
      filesProxy = projectProxy;
      this.customHeaders=customHeaders;
    }

    /// <summary>
    ///   Retrieves volume summary from Raptor and saves it to the landfill DB
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">Project</param>
    /// <param name="geofence">GeofenceResponse</param>
    /// <param name="entry">Weight entry from the client</param>
    /// <returns></returns>
    public async Task GetVolumeInBackground(string userUid, Project project, List<WGSPoint> geofence,DateEntry entry)
    {
      try
      {
       // Console.WriteLine("GetVolumeInBackground for project {0} date {1}", project.id, entry.date);
        Log.LogDebug("GetVolumeInBackground for project {0} date {1}", project.id, entry.date);

        var res = await GetVolumesAsync(userUid, project, entry.date, geofence);

        Log.LogDebug("Volume res:" + res);
        Log.LogDebug("Volume: " + res.Fill);

        LandfillDb.SaveVolume(project.projectUid, entry.geofenceUid, entry.date, res.Fill);
      }
      catch (RaptorApiException e)
      {
        if (e.code == HttpStatusCode.BadRequest)
        {
          // this response code is returned when the volume isn't available (e.g. the time range
          // is outside project extents); the assumption is that's the only reason we will
          // receive a 400 Bad Request 

          Log.LogWarning("RaptorApiException while retrieving volumes: " + e.Message);
          LandfillDb.MarkVolumeNotAvailable(project.projectUid, entry.geofenceUid, entry.date);
          LandfillDb.SaveVolume(project.projectUid, entry.geofenceUid, entry.date, 0);

          // TESTING CODE
          // Volume range in m3 should be ~ [478, 1020]
          //LandfillDb.SaveVolume(project.id, entry.date, new Random().Next(541) + 478, entry.geofenceUid);
        }
        else
        {
          Log.LogError("RaptorApiException while retrieving volumes: " + e.Message);
          LandfillDb.MarkVolumeNotRetrieved(project.projectUid, entry.geofenceUid, entry.date);
        }
      }
      catch (Exception e)
      {
        Log.LogError("Exception while retrieving volumes: " + e.Message);
        LandfillDb.MarkVolumeNotRetrieved(project.projectUid, entry.geofenceUid, entry.date);
      }
    }

    /// <summary>
    ///   Retrieves airspace volume summary information for a given project and date. This is the volume remaining
    ///   for the project calculated as the volume between the current ground surface and the design surface.
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">VisionLink project to retrieve volumes for</param>
    /// <param name="date">Date to retrieve volumes for (in project time zone)</param>
    /// <param name="returnEarliest">Flag to indicate if earliest or latest cell pass to be used</param>
    /// <returns>SummaryVolumesResult</returns>
    public async Task<SummaryVolumesResult> GetAirspaceVolumeAsync(string userUid, Project project,bool returnEarliest, int designId)
    {
      var tccFilespaceId = config.GetValueString("TCCfilespaceId");
      var topOfWasteDesignFilename = config.GetValueString("TopOfWasteDesignFilename");
      var volumeParams = new VolumeParams
      {
        projectId = project.id,
        volumeCalcType = 5,
        baseFilter = new VolumeFilter {returnEarliest = returnEarliest},
        topDesignDescriptor = new VolumeDesign
        {
          id = designId,
          file = new DesignDescriptor
          {
            filespaceId = tccFilespaceId,
            path = string.Format("/{0}/{1}", project.legacyCustomerID, project.id),
            fileName = topOfWasteDesignFilename
          }
        }
      };
      return await raptorProxy.ExecuteGenericV1Request<SummaryVolumesResult>("/volumes/summary", volumeParams, customHeaders);
    }

    public async Task<List<DesignDescriptiorLegacy>> GetDesignID(string jwt, Project project,string customerUid)
    {
      return (await filesProxy.GetFiles(project.projectUid, "", customHeaders)).Select(data =>
        new DesignDescriptiorLegacy()
        {
          fileType = data.ImportedFileType.ToString(),
          id = (int)data.LegacyFileId,
          name = data.Name
        }).ToList();
    }

    /// <summary>
    ///   Retrieves project statistics information for a given project.
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">VisionLink project to retrieve volumes for</param>
    /// <returns>ProjectStatisticsResult</returns>
    public async Task<ProjectStatisticsResult> GetProjectStatisticsAsync(string userUid,
      Project project)
    {
      var statsParams = new StatisticsParams {projectId = project.id};
      return await raptorProxy.ExecuteGenericV1Request<ProjectStatisticsResult>("/projects/statistics", statsParams, customHeaders);
    }


    public TimeZoneInfo GetTimeZoneInfoForTzdbId(string tzdbId)
    {
      var mappings = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones;
      var map = mappings.FirstOrDefault(x => x.TzdbIds.Any(z => z.Equals(tzdbId, StringComparison.OrdinalIgnoreCase)));
      return map == null ? null : TimeZoneInfo.FindSystemTimeZoneById(map.WindowsId);
    }

    /// <summary>
    /// Get timezone difference from UTC and return offset in minutes
    /// </summary>
    /// <param name="timeZone">Nodatime timezone</param>
    /// <returns>Offset in minutes</returns>
    public int ConvertFromTimeZoneToMinutesOffset(string timeZone)
    {
      var zone = DateTimeZoneProviders.Tzdb[timeZone];
      var offset = zone.GetUtcOffset(SystemClock.Instance.GetCurrentInstant());
      return offset.Milliseconds == 0 ? 0 : (offset.Milliseconds * -1) / 60000;
    }

    /// <summary>
    ///   Retrieves CCA summary from Raptor and saves it to the landfill DB
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">Project</param>
    /// <param name="geofence">GeofenceResponse boundary</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="date">Date to retrieve CCA for (in project time zone)</param>
    /// <param name="machineId">Landfill Machine ID</param>
    /// <param name="machine">Machine details</param>
    /// <param name="liftId">Lift/layer number. If not specified then CCA retrieved for all lifts</param>
    /// <returns></returns>
    public async Task GetCCAInBackground(string userUid, Project project, string geofenceUid,
      List<WGSPoint> geofence, DateTime date, long machineId, MachineDetails machine, int? liftId)
    {
      try
      {
        Log.LogDebug("Get CCA for projectId {0} date {1} machine name {2} machine id {3} geofenceUid {4} liftId {5}",
          project.id, date, machine.machineName, machineId, geofenceUid, liftId);

        var res = await GetCCAAsync(userUid, project, date, machine, liftId, geofence);

        Log.LogDebug("CCA res:" + res);
        Log.LogDebug("CCA: incomplete {0}, complete {1}, overcomplete {2}", res.undercompletePercent,
          res.completePercent, res.overcompletePercent);

        LandfillDb.SaveCCA(project.projectUid, geofenceUid, date, machineId, liftId, res.undercompletePercent,
          res.completePercent, res.overcompletePercent);
      }
      catch (RaptorApiException e)
      {
        if (e.code == HttpStatusCode.BadRequest)
        {
          // this response code is returned when the CCA isn't available (e.g. the time range
          // is outside project extents); the assumption is that's the only reason we will
          // receive a 400 Bad Request 

          Log.LogWarning("RaptorApiException while retrieving CCA: " + e.Message);
          LandfillDb.MarkCCANotAvailable(project.projectUid, geofenceUid, date, machineId, liftId);
        }
        else
        {
          Log.LogError("RaptorApiException while retrieving CCA: " + e.Message);
          LandfillDb.MarkCCANotRetrieved(project.projectUid, geofenceUid, date, machineId, liftId);
        }
      }
      catch (Exception e)
      {
        Log.LogError("Exception while retrieving CCA: " + e.Message);
        LandfillDb.MarkCCANotRetrieved(project.projectUid, geofenceUid, date, machineId, liftId);
      }
    }

    /// <summary>
    ///   Retrieves a list of machines and lifts for the project for the given date range.
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">Project</param>
    /// <param name="startDate">Start date in project time zone</param>
    /// <param name="endDate">End date in project time zone</param>
    /// <returns>List of machines and lifts in project time zone</returns>
    public async Task<List<MachineLifts>> GetMachineLiftsInBackground(string userUid, Project project,
      DateTime startDate, DateTime endDate)
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
          Log.LogWarning("RaptorApiException while retrieving machines & lifts: " + e.Message);
      }
      catch (Exception e)
      {
        Log.LogError("Exception while retrieving machines & lifts: " + e.Message);
      }

      return new List<MachineLifts>();
    }

    

    /// <summary>
    ///   Retrieves volume summary information for a given project, date and geofence
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">VisionLink project to retrieve volumes for</param>
    /// <param name="date">Date to retrieve volumes for (in project time zone)</param>
    /// <param name="geofence">
    ///   GeofenceResponse to retrieve volumes for. If not specified then volume retrieved for entire
    ///   project area
    /// </param>
    /// <returns>Summary volumes</returns>
    private async Task<SummaryVolumesResult> GetVolumesAsync(string userUid, Project project,
      DateTime date, List<WGSPoint> geofence)
    {
      DateTime startUtc;
      DateTime endUtc;
      ConvertToUtc(date, project.timeZoneName, out startUtc, out endUtc);
      Log.LogDebug("UTC time range in Volume request: {0} - {1}", startUtc, endUtc);

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

      //var logVolumeParams = JsonConvert.SerializeObject(volumeParams, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      //Console.WriteLine("VOLUMES=" + logVolumeParams);
      var result = await raptorProxy.ExecuteGenericV1Request<SummaryVolumesResult>("/volumes/summary", volumeParams, customHeaders);
      
      //Log.LogDebug("Volumes request for project {0}: {1} {2} Result : {3}", project.id, reportEndpoint,JsonConvert.SerializeObject(volumeParams), JsonConvert.SerializeObject(result));
      return result;
    }

    private async Task<ProjectExtentsResult> GetProjectExtentsAsync(string userUid, Project project)
    {
      Log.LogDebug("In GetProjectExtentsAsync");

      var volumeParams = new ProjectExtentsParams
      {
        projectId = project.id,
        excludedSurveyedSurfaceIds = new int[0]
      };

      return await raptorProxy.ExecuteGenericV1Request<ProjectExtentsResult>("/projects/statistics", volumeParams, customHeaders);
    }

    private void ConvertToUtc(DateTime date, string timeZoneName, out DateTime startUtc, out DateTime endUtc)
    {
      //var projTimeZone = DateTimeZoneProviders.Tzdb[timeZoneName];
      //var utcNow = DateTime.UtcNow;
      //var projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
      //use only utc dates and times in the service contracts. Ignore time for now.
      var offset = ConvertFromTimeZoneToMinutesOffset(timeZoneName);
      //var utcDateTime = date.Date.Add(projTimeZoneOffsetFromUtc.ToTimeSpan().Negate());
      var utcDateTime = date.Date.AddMinutes(offset);
      startUtc = utcDateTime;
      endUtc = utcDateTime.AddDays(1).AddMinutes(-1);
    }

    /// <summary>
    ///   Retrieves CCA summary information for a given project, date and machine
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">VisionLink project to retrieve volumes for</param>
    /// <param name="date">Date to retrieve CCA for (in project time zone)</param>
    /// <param name="machine">Machine to retrieve CCA for</param>
    /// <param name="geofence">
    ///   GeofenceResponse to retrieve CCA for. If not specified then CCA retrieved for entire
    ///   project area
    /// </param>
    /// <param name="liftId">Lift/layer number to retrieve CCA for. If not specified then CCA retrieved for all lifts</param>
    /// <returns>CCASummaryResult</returns>
    private async Task<CCASummaryResult> GetCCAAsync(string userUid, Project project, DateTime date,
      MachineDetails machine, int? liftId, List<WGSPoint> geofence)
    {
      DateTime startUtc;
      DateTime endUtc;
      ConvertToUtc(date, project.timeZoneName, out startUtc, out endUtc);
      Log.LogDebug("UTC time range in CCA request: {0} - {1}", startUtc, endUtc);

      //This is because we sometimes pass MachineLiftDetails and the serialization
      //will do the derived class and RaptorServices complains about the extra properties.
      var details = new MachineDetails
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
          contributingMachines = new List<MachineDetails> {details},
          layerNumber = liftId,
          layerType = liftId.HasValue ? 7 : (int?) null, //7 = TagFile!
          polygonLL = geofence
        }
      };

      return await raptorProxy.ExecuteGenericV1Request<CCASummaryResult>("/compaction/cca/summary", ccaParams, customHeaders);
    }

    /// <summary>
    ///   Retrieves a list of machines and lifts for the project for the given datetime range.
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">Project to retrieve machines and lifts for</param>
    /// <param name="startUtc">Start UTC to retrieve machines and lifts for</param>
    /// <param name="endUtc">End UTC to retrieve machines and lifts for</param>
    /// <returns>Machines and lifts</returns>
    private async Task<MachineLayerIdsExecutionResult> GetMachineLiftListAsync(string userUid,Project project, DateTime startUtc, DateTime endUtc)
    {
      var url = $"/projects/{project.id}/machinelifts";
      var query = $"?startUtc={FormatUtcDate(startUtc)}&endUtc={FormatUtcDate(endUtc)}";
      //Console.WriteLine("GetMachineLiftList: Url = {0} {1}", url,query);
      return await raptorProxy.ExecuteGenericV1Request<MachineLayerIdsExecutionResult>(url, query, customHeaders);

    }

    /// <summary>
    ///   Converts the list of machines and lifts from Raptor to the list for the Web API.
    ///   Raptor can have multiple entries per day for a lift whereas the Web API only wants one.
    ///   Also Raptor uses UTC while the Web API uses the project time zone.
    ///   Finally Raptor lifts can continue past the end of the day while the Web API wants to stop at the end of the day.
    /// </summary>
    /// <param name="project">The project for which the machine/lifts conversion is occurring.</param>
    /// <param name="endUtc">The start UTC for the machine/lifts</param>
    /// <param name="machineList">The list of machines and lifts returned by Raptor</param>
    /// <returns>List of machines and lifts in project time zone.</returns>
    private List<MachineLifts> GetMachineLiftsInProjectTimeZone(Project project, DateTime endUtc,IEnumerable<MachineLiftDetails> machineList)
    {
      //var hwZone = GetTimeZoneInfoForTzdbId(project.timeZoneName);
      var offset = ConvertFromTimeZoneToMinutesOffset(project.timeZoneName);
      var machineLifts = new List<MachineLifts>();
      foreach (var machine in machineList)
      {
        var machineLift = new MachineLifts
        {
          assetId = machine.assetId,
          machineName = machine.machineName,
          isJohnDoe = machine.isJohnDoe
        };
        //Only want last lift of the day for each lift
        var orderedLifts = machine.lifts.OrderBy(l => l.layerId).ThenByDescending(l => l.endUtc);
        var lifts = new List<Lift>();
        foreach (var orderedLift in orderedLifts)
          if (lifts.Where(l => l.layerId == orderedLift.layerId).FirstOrDefault() == null)
          {
            //If the lift is still active at the end of the day the use the end of the day
            if (orderedLift.endUtc > endUtc)
              orderedLift.endUtc = endUtc;
            lifts.Add(new Lift {layerId = orderedLift.layerId, endTime = orderedLift.endUtc.AddMinutes(offset)});   //.Add(hwZone.BaseUtcOffset)});
          }

        machineLift.lifts = lifts;
        machineLifts.Add(machineLift);
      }

      return machineLifts;
    }

    /// <summary>
    ///   Formats UTC date in ISO 8601 format for Raptor Services Web API.
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