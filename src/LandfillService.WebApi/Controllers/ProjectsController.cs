using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using LandfillService.Common.ApiClients;
using LandfillService.Common.Contracts;
using LandfillService.Common;
using LandfillService.Common.Models;
using Newtonsoft.Json;
using NodaTime;
using System.Reflection;
using Common.Repository;
using LandfillService.WebApi.Auth;

namespace LandfillService.WebApi.Controllers
{
  /// <summary>
  /// Handles projectResponse related requests
  /// </summary>
  [RoutePrefix("api/v2/projects")]
  public class ProjectsController : ApiController
  {
    private RaptorApiClient raptorApiClient = new RaptorApiClient();

    #region Projects

    /// <summary>
    /// Retrieves a list of projects from the db
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="customerUid">User ID</param>
    /// <returns>A list of projects or error details</returns>
    private IEither<IHttpActionResult, IEnumerable<ProjectResponse>> PerhapsUpdateProjectList(string userUid,
      string customerUid)
    {
      IEnumerable<ProjectResponse> projects = LandfillDb.GetProjects(userUid, customerUid);
      //LoggerSvc.LogMessage(null, null, null, "PerhapsUpdateProjectList: projects count=" + projects.Count());
      return Either.Right<IHttpActionResult, IEnumerable<ProjectResponse>>(projects);
    }

    /// <summary>
    /// Returns the list of projects available to the user
    /// </summary>
    /// <returns>List of available projects</returns>
    [Route("")]
    public IHttpActionResult Get()
    {
      var principal = (RequestContext.Principal as LandfillPrincipal);
      return PerhapsUpdateProjectList(principal.UserUid, principal.CustomerUid)
        .Case(errorResponse => errorResponse, projects => Ok(projects));
    }

    /// <summary>
    /// TEST CODE: generate random projectResponse data entries 
    /// </summary>
    /// <returns>Random projectResponse data entries</returns>
    private IEnumerable<DayEntry> GetRandomEntries()
    {
      var totalDays = 730;
      var startDate = DateTime.Today.AddDays(-totalDays);

      var entries = new List<DayEntry>();

      var rnd = new Random();

      var densityExtra = rnd.Next(1, 3);
      var weightExtra = rnd.Next(200, 300);


      foreach (int i in Enumerable.Range(0, totalDays))
      {
        bool skip = (i < 728 && rnd.Next(5) % 6 == 0);

        double density = skip ? 0 : rnd.Next(1200 / densityExtra, 1600 / densityExtra);
        double weight = skip ? 0 : rnd.Next(500, 800 + weightExtra);
        entries.Add(new DayEntry
        {
          date = DateTime.Today.AddDays(-totalDays + i),
          entryPresent = !skip,
          weight = weight,
          volume = skip ? 0 : weight * 1000 / density
        });
      }
      return entries.ToArray();
    }

    /// <summary>
    /// Returns the projectResponse data for the given projectResponse. If geofenceUid is not specified, 
    /// data for the entire projectResponse area is returned otherwise data for the geofenced area is returned.
    /// If no date range specified, returns data for the last 2 years to today in the projectResponse time zone
    /// otherwise returns data for the specified date range.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="startDate">Start date in projectResponse time zone for which to return data</param>
    /// <param name="endDate">End date in projectResponse time zone for which to return data</param>
    /// <returns>List of data entries for each day in date range and the status of volume retrieval for the projectResponse</returns>
    [Route("{id}")]
    public IHttpActionResult Get(uint id, Guid? geofenceUid = null, DateTime? startDate = null,
      DateTime? endDate = null)
    {
      // Get the available data
      // Kick off missing volumes retrieval IF not already running
      // Check if there are missing volumes and indicate to the client
      var principal = (RequestContext.Principal as LandfillPrincipal);

      //Secure with projectResponse list
      if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }
      LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),
        "Retrieving density");

      return PerhapsUpdateProjectList(principal.UserUid, principal.CustomerUid).Case(errorResponse => errorResponse,
        projects =>
        {
          try
          {
            var project = projects.First(p => p.id == id);
            //  GetMissingVolumesInBackground(userUid, projectResponse);  // retry volume requests which weren't successful before
            var entries = new ProjectData
            {
              projectResponse = project,
              entries = LandfillDb.GetEntries(project, geofenceUid.HasValue ? geofenceUid.ToString() : null, startDate,
                endDate),
              retrievingVolumes = false //  todo LandfillDb.RetrievalInProgress(projectResponse)
            };

            return Ok(entries);
            // TEST CODE: use this to test chart updates on the client
            //return Ok(new ProjectData { entries = GetRandomEntries(), retrievingVolumes = true });
          }
          catch (InvalidOperationException)
          {
            return Ok();
          }
        });

    }

    #endregion

    #region Weights

    /// <summary>
    /// Returns the weights for all geofences for the projectResponse for the date range 
    /// of the last 2 years to today in the projectResponse time zone.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>List of entries for each day in date range and the weight for each geofence for that day</returns>
    [Route("{id}/weights")]
    public IHttpActionResult GetWeights(uint id)
    {
      var principal = (RequestContext.Principal as LandfillPrincipal);

      //Secure with projectResponse list
      if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }
      LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),
        "Retrieving weights");

      return PerhapsUpdateProjectList(principal.UserUid, principal.CustomerUid).Case(errorResponse => errorResponse,
        projects =>
        {
          try
          {
            var project = projects.Where(p => p.id == id).First();

            var data = new WeightData
            {
              projectResponse = project,
              entries = GetGeofenceWeights(project),
              retrievingVolumes = false // todo LandfillDb.RetrievalInProgress(projectResponse)
            };

            return Ok(data);

          }
          catch (InvalidOperationException)
          {
            return Ok();
          }
        });

    }

    /// <summary>
    /// Retrieves weights entries for each geofence in the projectResponse.
    /// </summary>
    /// <param name="projectResponse">Project</param>
    /// <returns>List of dates with the weight for each geofence for that date</returns>
    private List<GeofenceWeightEntry> GetGeofenceWeights(ProjectResponse projectResponse)
    {
      Dictionary<DateTime, List<GeofenceWeight>> weights = new Dictionary<DateTime, List<GeofenceWeight>>();
      IEnumerable<Guid> geofenceUids = LandfillDb.GetGeofences(projectResponse.projectUid).Select(g => g.uid);
      foreach (var geofenceUid in geofenceUids)
      {
        var entries = LandfillDb.GetEntries(projectResponse, geofenceUid.ToString(), null, null);
        foreach (var entry in entries)
        {
          if (!weights.ContainsKey(entry.date))
            weights.Add(entry.date, new List<GeofenceWeight>());
          weights[entry.date].Add(new GeofenceWeight {geofenceUid = geofenceUid, weight = entry.weight});
        }
      }

      return (from w in weights
        select new GeofenceWeightEntry
        {
          date = w.Key,
          entryPresent = w.Value.Any(v => v.weight != 0),
          geofenceWeights = w.Value
        }).ToList();
    }

    /// <summary>
    /// Saves weights submitted in the request.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <returns>Project data and status of volume retrieval</returns>
    [HttpPost]
    [Route("{id}/weights")]
    public IHttpActionResult PostWeights(uint id, Guid? geofenceUid = null /*, [FromBody] WeightEntry[] entries*/)
    {
      //When the request goes through TPaaS the headers get changed to Transfer-Encoding: chunked and the Content-Length is 0.
      //For some reason the Web API framework can't handle this and doesn't deserialize the 'entries'.
      //So we do it manually here. Note this problem only occurs when URI and body contain parameters.
      //See http://w3foverflow.com/question/asp-net-web-api-the-framework-is-not-converting-json-to-object-when-using-chunked-transfer-encoding/
      //If you hit the Landfill service directly it all works.
      string jsonContent = Request.Content.ReadAsStringAsync().Result; //this gets proper JSON
      //LoggerSvc.LogMessage(null, null, null, "PostWeights: id=" + id + ", request content=" + jsonContent);          
      var entries = JsonConvert.DeserializeObject<WeightEntry[]>(jsonContent);

      //Secure with projectResponse list
      if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }


      if (entries == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
            "Missing weight entries"));
      }

      if (!geofenceUid.HasValue || geofenceUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
            "Missing geofence UID"));
      }
      var geofenceUidStr = geofenceUid.Value.ToString();

      var principal = (RequestContext.Principal as LandfillPrincipal);
      //LoggerSvc.LogMessage(null, null, null, "PostWeights: userUid=" + userUid);          


      return PerhapsUpdateProjectList(principal.UserUid, principal.CustomerUid).Case(errorResponse => errorResponse,
        projects =>
        {
          var project = projects.Where(p => p.id == id).First();

          var projTimeZone = DateTimeZoneProviders.Tzdb[project.timeZoneName];

          DateTime utcNow = DateTime.UtcNow;
          Offset projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
          DateTime yesterdayInProjTimeZone = (utcNow + projTimeZoneOffsetFromUtc.ToTimeSpan()).AddDays(-1);

          System.Diagnostics.Debug.WriteLine("yesterdayInProjTimeZone=" + yesterdayInProjTimeZone.ToString());

          var validEntries = new List<DateEntry>();
          foreach (var entry in entries)
          {
            bool valid = entry.weight >= 0 && entry.date.Date <= yesterdayInProjTimeZone.Date;
            System.Diagnostics.Debug.WriteLine(entry.ToString() + "--->" + valid);

            if (valid)
            {
              LandfillDb.SaveEntry(project, geofenceUidStr, entry);
              validEntries.Add(new DateEntry {date = entry.date, geofenceUid = geofenceUidStr});
            }
          }

          System.Diagnostics.Debug.WriteLine("Finished posting weights");

          return Ok(new WeightData
          {
            projectResponse = project,
            entries = GetGeofenceWeights(project),
            retrievingVolumes = true
          });

        });
    }

    #endregion

    #region Volumes

    /// <summary>
    /// Gets volume and time summary for a landfill projectResponse.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Current week volume, current month volume, remaining volume (air space) and time remaining (days)</returns>
    [Route("{id}/volumeTime")]
    public async Task<IHttpActionResult> GetVolumeTimeSummary(uint id)
    {
      var principal = (RequestContext.Principal as LandfillPrincipal);
      //Secure with projectResponse list
      if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }
      LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),
        "Retrieving Volume/Time");

      try
      {
        var projects = LandfillDb.GetProjects(principal.UserUid, principal.CustomerUid);
        var project = projects.First(p => p.id == id);
        DateTime todayinProjTimeZone = LandfillDb.GetTodayInProjectTimeZone(project.timeZoneName);
        var startWeek = CurrentWeekMonday(todayinProjTimeZone);
        var weekVol = LandfillDb.GetEntries(project, null, startWeek, todayinProjTimeZone).Sum(e => e.volume);
        var startMonth = new DateTime(todayinProjTimeZone.Year, todayinProjTimeZone.Month, 1);
        var monthVol = LandfillDb.GetEntries(project, null, startMonth, todayinProjTimeZone).Sum(e => e.volume);
        //Get designIds from ProjectMonitoring service
        var res = await raptorApiClient.GetDesignID((RequestContext.Principal as LandfillPrincipal).JWT, project,
          (RequestContext.Principal as LandfillPrincipal).CustomerUid);
        var designId = res.Where(r => r.name == "TOW.ttm").Select(i => i.id).First();
        LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Design id: " + designId.ToString(),
          "Retrieving DesignID");
        var firstAirspaceVol = await GetAirspaceVolumeInBackground(principal.UserUid, project, true, designId);
        var lastAirspaceVol = await GetAirspaceVolumeInBackground(principal.UserUid, project, false, designId);
        var statsDates = await GetProjectStatisticsInBackground(principal.UserUid, project);
        var dates = statsDates.ToList();
        var volPerDay = firstAirspaceVol.HasValue && lastAirspaceVol.HasValue
          ? Math.Abs(firstAirspaceVol.Value - lastAirspaceVol.Value) /
            Math.Abs((dates[0] - dates[1]).TotalDays)
          : (double?) null;
        var timeLeft = volPerDay.HasValue ? lastAirspaceVol / volPerDay.Value : (double?) null;
        return Ok(new VolumeTime
        {
          currentWeekVolume = weekVol,
          currentMonthVolume = monthVol,
          remainingVolume = lastAirspaceVol,
          remainingTime = timeLeft
        });

      }
      catch (InvalidOperationException)
      {
        return Ok();
      }
    }

    /// <summary>
    /// Returns the date representing the Monday of the week the given date falls in.
    /// </summary>
    /// <param name="date">The date for which to find the start of the week</param>
    /// <returns>The date of the Monday of that week</returns>
    private static DateTime CurrentWeekMonday(DateTime date)
    {
      int daysToSubtract = 0;
      switch (date.DayOfWeek)
      {
        case DayOfWeek.Sunday:
          daysToSubtract = 6;
          break;
        case DayOfWeek.Saturday:
          daysToSubtract = 5;
          break;
        case DayOfWeek.Friday:
          daysToSubtract = 4;
          break;
        case DayOfWeek.Thursday:
          daysToSubtract = 3;
          break;
        case DayOfWeek.Wednesday:
          daysToSubtract = 2;
          break;
        case DayOfWeek.Tuesday:
          daysToSubtract = 1;
          break;
        case DayOfWeek.Monday:
          daysToSubtract = 0;
          break;
      }
      return date.AddDays(-daysToSubtract);
    }

    /// <summary>
    /// Retrieves airspace volume summary from Raptor.
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="projectResponse">Project</param>
    /// <param name="returnEarliest">Indicates if filtering by earliest or latest cell pass</param>
    /// <param name="designId"></param>
    /// <returns></returns>
    private async Task<double?> GetAirspaceVolumeInBackground(string userUid, ProjectResponse projectResponse, bool returnEarliest,
      int designId)
    {
      try
      {
        var res = await raptorApiClient.GetAirspaceVolumeAsync(userUid, projectResponse, returnEarliest, designId);
        System.Diagnostics.Debug.WriteLine("Airspace Volume res:" + res);
        System.Diagnostics.Debug.WriteLine("Airspace Volume: " + res.Fill);

        return res.Fill;
      }
      catch (RaptorApiException e)
      {
        if (e.code == HttpStatusCode.BadRequest)
        {
          // this response code is returned when the summary volumes isn't available (e.g. 
          // the design file is not there); the assumption is that's the only reason we will
          // receive a 400 Bad Request 

          LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + projectResponse.id,
            "RaptorApiException while retrieving airspace volume: " + e.Message);
          return (double?) null;
        }
        else
        {
          LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + projectResponse.id,
            "RaptorApiException while retrieving airspace volume: " + e.Message);
          throw;
        }
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Write("Exception while retrieving airspace volume: " + e);
        throw;
      }
    }

    /// <summary>
    /// Retrieves projectResponse statistics from Raptor.
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="projectResponse">Project</param>
    /// <returns>Date extents from projectResponse statistics</returns>
    private async Task<IEnumerable<DateTime>> GetProjectStatisticsInBackground(string userUid, ProjectResponse projectResponse)
    {
      try
      {
        var res = await raptorApiClient.GetProjectStatisticsAsync(userUid, projectResponse);

        System.Diagnostics.Debug.WriteLine("Statistics res:" + res);
        System.Diagnostics.Debug.WriteLine("Statistics dates: " + res.startTime + " - " + res.endTime);
        return new List<DateTime> {res.startTime, res.endTime};
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Write("Exception while retrieving statistics: " + e);
        throw;
      }
    }

    #endregion

    #region Geofences

    /// <summary>
    /// Returns a list of geofences for the projectResponse. A geofence is associated with a projectResponse if its
    /// boundary is inside or intersects that of the projectResponse and it is of type 'Landfill'. The projectResponse
    /// geofence is also returned.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>List of geofences with their bounding boxes</returns>
    [Route("{id}/geofences")]
    public IHttpActionResult GetGeofences(uint id)
    {
      var principal = (RequestContext.Principal as LandfillPrincipal);
      //Secure with projectResponse list
      if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }
      LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),
        "Retrieving geofences");

      try
      {
        var project = LandfillDb.GetProjects(principal.UserUid, principal.CustomerUid).Where(p => p.id == id).First();
        IEnumerable<GeofenceResponse> geofences = LandfillDb.GetGeofences(project.projectUid);
        return Ok(geofences);
      }
      catch (InvalidOperationException)
      {
        return Ok();
      }
    }

    /// <summary>
    /// Returns a geofence boundary.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <returns>List of WGS84 boundary points in radians</returns>
    [Route("{id}/geofences/{geofenceUid}")]
    public IHttpActionResult GetGeofenceBoundary(uint id, Guid geofenceUid)
    {
      var userUid = (RequestContext.Principal as LandfillPrincipal).UserUid;
      //Secure with projectResponse list
      if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }
      string geofenceUidStr = geofenceUid.ToString();
      LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),
        "Retrieving geofence boundary for " + geofenceUidStr);

      try
      {
        IEnumerable<WGSPoint> points = LandfillDb.GetGeofencePoints(geofenceUidStr);
        return Ok(points);
      }
      catch (InvalidOperationException)
      {
        return Ok();
      }
    }

    private Dictionary<string, List<WGSPoint>> GetGeofenceBoundaries(uint id, List<string> geofenceUids)
    {
      Dictionary<string, List<WGSPoint>> geofences = geofenceUids.ToDictionary(g => g,
        g => LandfillDb.GetGeofencePoints(g).ToList());
      LoggerSvc.LogMessage(null, null, null,
        string.Format("Got {0} geofences to process for projectResponse {1}", geofenceUids.Count, id));

      return geofences;
    }

    #endregion

    #region CCA

    /// <summary>
    /// Gets CCA ratio data on a daily basis for a landfill projectResponse for all machines. If geofenceUid is not specified, 
    /// CCA ratio data for the entire projectResponse area is returned otherwise CCA ratio data for the geofenced area is returned.
    /// If no date range specified, returns CCA ratio data for the last 2 years to today in the projectResponse time zone
    /// otherwise returns CCA ratio data for the specified date range.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="startDate">Start date in projectResponse time zone for which to return data</param>
    /// <param name="endDate">End date in projectResponse time zone for which to return data</param>
    /// <returns>List of machines and daily CCA ratio</returns>
    [Route("{id}/ccaratio")]
    public IHttpActionResult GetCCARatio(uint id, Guid? geofenceUid = null, DateTime? startDate = null,
      DateTime? endDate = null)
    {
      var principal = (RequestContext.Principal as LandfillPrincipal);
      //Secure with projectResponse list
      if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }
      LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),
        "Retrieving CCA Ratio");

      try
      {
        var project = LandfillDb.GetProjects(principal.UserUid, principal.CustomerUid).First(p => p.id == id);
        var ccaData = LandfillDb.GetCCA(project, geofenceUid.HasValue ? geofenceUid.ToString() : null, startDate,
          endDate, null, null);
        var groupedData = ccaData.GroupBy(c => c.machineId).ToDictionary(k => k.Key, v => v.ToList());
        var machines = groupedData.ToDictionary(k => k.Key, v => LandfillDb.GetMachine(v.Key));
        var data = groupedData.Select(d => new CCARatioData
        {
          machineName = machines[d.Key] == null ? "Unknown" : machines[d.Key].machineName,
          entries = groupedData[d.Key].Select(v => new CCARatioEntry
          {
            date = v.date,
            ccaRatio = Math.Round(v.complete + v.overcomplete)
          }).ToList()
        }).ToList();
        return Ok(data);
      }
      catch (InvalidOperationException)
      {
        return Ok();
      }
    }

    /// <summary>
    /// Gets CCA summary data for a landfill projectResponse for the specified date. If geofenceUid is not specified, 
    /// CCA summary data for the entire projectResponse area is returned otherwise CCA data for the geofenced area is returned.
    /// If machine (asset ID, machine name and John Doe flag) is not specified, returns data for all machines 
    /// else for the specified machine. If lift ID is not specified returns data for all lifts else for the specified lift.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="date">Date in projectResponse time zone for which to return data</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="assetId">Asset ID (from MachineDetails)</param>
    /// <param name="machineName">Machine name (from MachineDetails)</param>
    /// <param name="isJohnDoe">IsJohnDoe flag (from MachineDetails)</param>
    /// <param name="liftId">Lift/Layer ID</param>
    /// <returns>CCA summary for the date</returns>
    [Route("{id}/ccasummary")]
    public IHttpActionResult GetCCASummary(uint id, DateTime? date, Guid? geofenceUid = null,
      uint? assetId = null, string machineName = null, bool? isJohnDoe = null, int? liftId = null)
    {
      //NOTE: CCA summary is not cumulative. 
      //If data for more than one day is required, client must call Raptor service directly

      var principal = (RequestContext.Principal as LandfillPrincipal);
      //Secure with projectResponse list
      if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }
      LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),
        "Retrieving CCA Summary");

      if (!date.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
            "Missing date"));
      }

      bool gotMachine = assetId.HasValue && isJohnDoe.HasValue && !string.IsNullOrEmpty(machineName);
      bool noMachine = !assetId.HasValue && !isJohnDoe.HasValue && string.IsNullOrEmpty(machineName);
      if (!gotMachine && !noMachine)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
            "Either all or none of the machine details parameters must be provided"));
      }

      try
      {
        var project = LandfillDb.GetProjects(principal.UserUid, principal.CustomerUid).First(p => p.id == id);
        long? machineId = noMachine
          ? (long?) null
          : LandfillDb.GetMachineId(project.projectUid,
            new MachineDetails
            {
              assetId = assetId.Value,
              machineName = machineName,
              isJohnDoe = isJohnDoe.Value
            });

        if (gotMachine && machineId.Value == 0)
        {
          LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),
            "Failed to find machine");
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
              "Machine does not exist"));
        }
        var ccaData = LandfillDb.GetCCA(project, geofenceUid.HasValue ? geofenceUid.ToString() : null, date, date,
          machineId, liftId);
        var groupedData = ccaData.GroupBy(c => c.machineId).ToDictionary(k => k.Key, v => v.ToList());
        var machines = groupedData.ToDictionary(k => k.Key, v => LandfillDb.GetMachine(v.Key));

        var data = ccaData.Select(d => new CCASummaryData()
        {
          machineName = machines[d.machineId] == null ? "Unknown" : machines[d.machineId].machineName,
          liftId = d.liftId,
          incomplete = Math.Round(d.incomplete),
          complete = Math.Round(d.complete),
          overcomplete = Math.Round(d.overcomplete)
        }).ToList();
        return Ok(data);
      }
      catch (InvalidOperationException)
      {
        return Ok();
      }
    }

    /// <summary>
    /// Gets a list of machines and lifts for a landfill projectResponse. If no date range specified, 
    /// the last 2 years to today in the projectResponse time zone is used.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="startDate">Start date in projectResponse time zone for which to return data</param>
    /// <param name="endDate">End date in projectResponse time zone for which to return data</param>
    /// <returns>List of machines and lifts in projectResponse time zone</returns>
    [Route("{id}/machinelifts")]
    public async Task<IHttpActionResult> GetMachineLifts(uint id, DateTime? startDate = null, DateTime? endDate = null)
    {
      var principal = (RequestContext.Principal as LandfillPrincipal);
      //Secure with projectResponse list
      if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }
      LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),
        "Retrieving Machines and lifts");

      try
      {

        var project = LandfillDb.GetProjects(principal.UserUid, principal.CustomerUid).First(p => p.id == id);
        //    var projectResponse = LandfillDb.GetProject(id).First(); 
        var projTimeZone = DateTimeZoneProviders.Tzdb[project.timeZoneName];

        DateTime utcNow = DateTime.UtcNow;
        Offset projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
        if (!endDate.HasValue)
          endDate = (utcNow + projTimeZoneOffsetFromUtc.ToTimeSpan()).Date; //today in projectResponse time zone
        if (!startDate.HasValue)
          startDate = endDate.Value.AddYears(-2);
        var task = await raptorApiClient.GetMachineLiftsInBackground(null, project, startDate.Value, endDate.Value);
        return Ok(task);
      }
      catch (InvalidOperationException)
      {
        return Ok();
      }
    }

    #endregion
  }
}
