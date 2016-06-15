using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using LandfillService.Common.ApiClients;
using LandfillService.Common.Context;
using LandfillService.Common.Contracts;
using System.Web.Hosting;
using LandfillService.Common;
using LandfillService.Common.Models;
using Newtonsoft.Json;
using NodaTime;
using System.Reflection;
using VSS.VisionLink.Utilization.WebApi.Configuration;

namespace LandfillService.WebApi.Controllers
{
    /// <summary>
    /// Handles project related requests
    /// </summary>
    [System.Web.Http.RoutePrefix("api/v2/projects")]
    public class ProjectsController : ApiController
    {
        private RaptorApiClient raptorApiClient = new RaptorApiClient();

        //public ProjectsController()
        //{
        //    LandfillDb.UnlockAllProjects();  // if the service terminates, some projects can be left locked for volume retrieval; unlock them
        //}

        #region Projects
        /// <summary>
        /// Retrieves a list of projects from the db
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <returns>A list of projects or error details</returns>
        private IEither<IHttpActionResult, IEnumerable<Project>> PerhapsUpdateProjectList(string userUid)
        {
           IEnumerable<Project> projects = LandfillDb.GetProjects(userUid);
           //LoggerSvc.LogMessage(null, null, null, "PerhapsUpdateProjectList: projects count=" + projects.Count());
           return Either.Right<IHttpActionResult, IEnumerable<Project>>(projects);
        }

        /// <summary>
        /// Returns the list of projects available to the user
        /// </summary>
        /// <returns>List of available projects</returns>
        [System.Web.Http.Route("")]
        public IHttpActionResult Get()
        {
          return PerhapsUpdateProjectList((RequestContext.Principal as LandfillPrincipal).UserUid).Case(errorResponse => errorResponse, projects => Ok(projects));
        }

        /// <summary>
        /// TEST CODE: generate random project data entries 
        /// </summary>
        /// <returns>Random project data entries</returns>
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
        /// Returns the project data for the given project. If geofenceUid is not specified, 
        /// data for the entire project area is returned otherwise data for the geofenced area is returned.
        /// If no date range specified, returns data for the last 2 years to today in the project time zone
        /// otherwise returns data for the specified date range.
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="startDate">Start date in project time zone for which to return data</param>
        /// <param name="endDate">End date in project time zone for which to return data</param>
        /// <returns>List of data entries for each day in date range and the status of volume retrieval for the project</returns>
        [System.Web.Http.Route("{id}")]
        public IHttpActionResult Get(uint id, Guid? geofenceUid=null, DateTime? startDate=null, DateTime? endDate=null)
        {
            // Get the available data
            // Kick off missing volumes retrieval IF not already running
            // Check if there are missing volumes and indicate to the client

          var userUid = (RequestContext.Principal as LandfillPrincipal).UserUid;
          //Secure with project list
          if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
          {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
          }
          LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(),"Retrieving density");

            return PerhapsUpdateProjectList(userUid).Case(errorResponse => errorResponse, projects => 
            {
                try
                {
                    var project = projects.Where(p => p.id == id).First();
                    //  GetMissingVolumesInBackground(userUid, project);  // retry volume requests which weren't successful before
                  var entries = new ProjectData
                                {
                                    project = project,
                                    entries = LandfillDb.GetEntries(project, geofenceUid.HasValue ? geofenceUid.ToString() : null, startDate, endDate),
                                    retrievingVolumes = LandfillDb.RetrievalInProgress(project)
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
        /// Returns the weights for all geofences for the project for the date range 
        /// of the last 2 years to today in the project time zone.
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>List of entries for each day in date range and the weight for each geofence for that day</returns>
        [System.Web.Http.Route("{id}/weights")]
        public IHttpActionResult GetWeights(uint id)
        {
          var userUid = (RequestContext.Principal as LandfillPrincipal).UserUid;
          //Secure with project list
          if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
          {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
          }
          LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(), "Retrieving weights");

          return PerhapsUpdateProjectList(userUid).Case(errorResponse => errorResponse, projects => 
          {
              try
              {
                var project = projects.Where(p => p.id == id).First();

                var data = new WeightData
                              {
                                  project = project,
                                  entries = GetGeofenceWeights(project),
                                  retrievingVolumes = LandfillDb.RetrievalInProgress(project)
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
        /// Retrieves weights entries for each geofence in the project.
        /// </summary>
        /// <param name="project">Project</param>
        /// <returns>List of dates with the weight for each geofence for that date</returns>
        private List<GeofenceWeightEntry> GetGeofenceWeights(Project project)
        {
          Dictionary<DateTime, List<GeofenceWeight>> weights = new Dictionary<DateTime, List<GeofenceWeight>>();
          IEnumerable<Guid> geofenceUids = LandfillDb.GetGeofences(project.id).Select(g => g.uid);
          foreach (var geofenceUid in geofenceUids)
          {
            var entries = LandfillDb.GetEntries(project, geofenceUid.ToString(), null, null);
            foreach (var entry in entries)
            {
              if (!weights.ContainsKey(entry.date))
                weights.Add(entry.date, new List<GeofenceWeight>());
              weights[entry.date].Add(new GeofenceWeight { geofenceUid = geofenceUid, weight = entry.weight });
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
        /// <param name="geofenceUid">Geofence UID</param>
        /// <returns>Project data and status of volume retrieval</returns>
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("{id}/weights")]
        public IHttpActionResult PostWeights(uint id, Guid? geofenceUid=null /*, [FromBody] WeightEntry[] entries*/)
        {
          //When the request goes through TPaaS the headers get changed to Transfer-Encoding: chunked and the Content-Length is 0.
          //For some reason the Web API framework can't handle this and doesn't deserialize the 'entries'.
          //So we do it manually here. Note this problem only occurs when URI and body contain parameters.
          //See http://w3foverflow.com/question/asp-net-web-api-the-framework-is-not-converting-json-to-object-when-using-chunked-transfer-encoding/
          //If you hit the Landfill service directly it all works.
          string jsonContent = Request.Content.ReadAsStringAsync().Result; //this gets proper JSON
          //LoggerSvc.LogMessage(null, null, null, "PostWeights: id=" + id + ", request content=" + jsonContent);          
          var entries = JsonConvert.DeserializeObject<WeightEntry[]>(jsonContent);

          //Secure with project list
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
 
          var userUid = (RequestContext.Principal as LandfillPrincipal).UserUid;
          //LoggerSvc.LogMessage(null, null, null, "PostWeights: userUid=" + userUid);          


            return PerhapsUpdateProjectList(userUid).Case(errorResponse => errorResponse, projects =>
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
                        validEntries.Add(new DateEntry{date = entry.date, geofenceUid = geofenceUidStr});
                    }
                };

                GetVolumesInBackground(userUid, project, validEntries, () =>
                {
                   // GetMissingVolumesInBackground(userUid, project);
                });

                System.Diagnostics.Debug.WriteLine("Finished posting weights");
        
                return Ok(new WeightData
                          {
                              project = project,
                              entries = GetGeofenceWeights(project),
                              retrievingVolumes = true
                          });                           

            });
        }
        #endregion

        #region Volumes

        /// <summary>
        /// Retrieves volume summary from Raptor and saves it to the landfill DB
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">Project</param>
        /// <param name="geofence">Geofence</param>
        /// <param name="entry">Weight entry from the client</param>
        /// <returns></returns>
        private async Task GetVolumeInBackground(string userUid, Project project, List<WGSPoint> geofence, DateEntry entry)
        {
          try
          {
            var res = await raptorApiClient.GetVolumesAsync(userUid, project, entry.date, geofence);

            System.Diagnostics.Debug.WriteLine("Volume res:" + res);
            System.Diagnostics.Debug.WriteLine("Volume: " + (res.Fill));

            LandfillDb.SaveVolume(project.projectUid, entry.geofenceUid, entry.date, res.Fill);
          }
          catch (RaptorApiException e)
          {
            if (e.code == HttpStatusCode.BadRequest)
            {
              // this response code is returned when the volume isn't available (e.g. the time range
              // is outside project extents); the assumption is that's the only reason we will
              // receive a 400 Bad Request 
              System.Diagnostics.Debug.Write("RaptorApiException while retrieving volumes: " + e);
              LandfillDb.MarkVolumeNotAvailable(project.projectUid, entry.geofenceUid, entry.date);

              // TESTING CODE
              // Volume range in m3 should be ~ [478, 1020]
              //LandfillDb.SaveVolume(project.id, entry.date, new Random().Next(541) + 478, entry.geofenceUid);
            }
            else
              LandfillDb.MarkVolumeNotRetrieved(project.projectUid, entry.geofenceUid, entry.date);
          }
          catch (Exception e)
          {
            System.Diagnostics.Debug.Write("Exception while retrieving volumes: " + e);
            LandfillDb.MarkVolumeNotRetrieved(project.projectUid, entry.geofenceUid, entry.date);
            throw;
          }
        }

        /// <summary>
        /// Retries volume summary retrieval from Raptor for volumes marked not retrieved
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">Project</param>
        /// <returns></returns>
        private void GetMissingVolumesInBackground(string userUid, Project project)
        {
          // get a "lock" on the project so that only a single background task at a time is retrieving 
          // missing volumes 
          var noRetrievalInProgress = LandfillDb.LockForRetrieval(project);

          if (noRetrievalInProgress)
          {
            var dates = LandfillDb.GetDatesWithVolumesNotRetrieved(project);
            System.Diagnostics.Debug.Write("Dates without volumes: {0}", dates.ToString());
            GetVolumesInBackground(userUid, project, dates, () =>
            {
              var retrievalWasInProgress = LandfillDb.LockForRetrieval(project, false);  // "unlock" the project
              if (!retrievalWasInProgress)
                LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + project.id.ToString(),
                    "Project wasn't locked for retrieval when it should have been");
            });
          }
          else
            System.Diagnostics.Debug.Write("Retrieval of missing volumes already in progress");  // this would indicate a bug
        }

        /// <summary>
        /// Retrieves volumes via a background task in batches of 10 parallel requests
        /// </summary>
        /// <param name="userUid">User ID</param>
        /// <param name="project">Project</param>
        /// <param name="entries">Date entries (providing dates and geofence uids to request)</param>
        /// <param name="onComplete">Code to execute on completion</param>
        /// <returns></returns>
        private void GetVolumesInBackground(string userUid, Project project, IEnumerable<DateEntry> entries, Action onComplete)
        {
          HostingEnvironment.QueueBackgroundWorkItem(async (CancellationToken cancel) =>
          {
            const int parallelRequestCount = 1;

            var geofenceUids = entries.Where(d => !string.IsNullOrEmpty(d.geofenceUid)).Select(d => d.geofenceUid).Distinct().ToList();
            var geofences = GetGeofenceBoundaries(project.id, geofenceUids);

            for (var offset = 0; offset <= entries.Count() / parallelRequestCount; offset++)
            {
              var tasks = entries.Skip(offset * parallelRequestCount)
                .Take(parallelRequestCount)
                .Select(entry => GetVolumeInBackground(
                  userUid,
                  project,
                  geofences.ContainsKey(entry.geofenceUid) ? geofences[entry.geofenceUid] : null,
                  entry));
              await Task.WhenAll(tasks);
            }

            onComplete();
          });
        }

        /// <summary>
        /// Gets volume and time summary for a landfill project.
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Current week volume, current month volume, remaining volume (air space) and time remaining (days)</returns>
        [System.Web.Http.Route("{id}/volumeTime")]
        public IHttpActionResult GetVolumeTimeSummary(uint id)
        {
          var userUid = (RequestContext.Principal as LandfillPrincipal).UserUid;
          //Secure with project list
          if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
          {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
          }
          LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(), "Retrieving CCA %");

          try
          {
            //TODO: Implement this - for now we use Mock data
            VolumeTime data = new VolumeTime
                              {
                                currentWeekVolume = 3360,
                                currentMonthVolume = 12561,
                                remainingVolume = 68234765,
                                remainingTime = 1087
                              };

            return Ok(data);
          }
          catch (InvalidOperationException)
          {
            return Ok();
          }
        }
        #endregion

        #region Geofences

        /// <summary>
        /// Returns a list of geofences for the project. A geofence is associated with a project if its
        /// boundary is inside or intersects that of the project and it is of type 'Landfill'. The project
        /// geofence is also returned.
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>List of geofences</returns>
        [System.Web.Http.Route("{id}/geofences")]
        public IHttpActionResult GetGeofences(uint id)
        {
          var userUid = (RequestContext.Principal as LandfillPrincipal).UserUid;
          //Secure with project list
          if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
          {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
          }
          LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(), "Retrieving geofences");
         
          try
          {
            IEnumerable<Geofence> geofences = LandfillDb.GetGeofences(id);            
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
        /// <param name="geofenceUid">Geofence UID</param>
        /// <returns>List of WGS84 boundary points in radians</returns>
        [System.Web.Http.Route("{id}/geofences/{geofenceUid}")]
        public IHttpActionResult GetGeofenceBoundary(uint id, Guid geofenceUid)
        {
          var userUid = (RequestContext.Principal as LandfillPrincipal).UserUid;
          //Secure with project list
          if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
          {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
          }
          string geofenceUidStr = geofenceUid.ToString();
          LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(), "Retrieving geofence boundary for " + geofenceUidStr);

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
          LoggerSvc.LogMessage(null, null, null, string.Format("Got {0} geofences to process for project {1}", geofenceUids.Count, id));

          return geofences;
        }
        #endregion

        #region CCA
        /// <summary>
        /// Gets CCA summary for a landfill project.
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="startDate">Start date in project time zone for which to return data</param>
        /// <param name="endDate">End date in project time zone for which to return data</param>
        /// <returns>List of machines and daily CCA %</returns>
        [System.Web.Http.Route("{id}/cca")]
        public IHttpActionResult GetCCA(uint id, DateTime? startDate=null, DateTime? endDate=null)
        {
          var userUid = (RequestContext.Principal as LandfillPrincipal).UserUid;
          //Secure with project list
          if (!(RequestContext.Principal as LandfillPrincipal).Projects.ContainsKey(id))
          {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
          }
          LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + id.ToString(), "Retrieving CCA %");

          try
          {
            //TODO: Implement this - for now we use Mock data
            IEnumerable<CCAData> data = new List<CCAData>
                {
                  new CCAData
                  {
                    machineName = "CAT11",
                    entries = new List<CCAEntry>
                    {
                      new CCAEntry{ date = new DateTime(2016,02,01), ccaPercent = 15.2 },
                      new CCAEntry{ date = new DateTime(2016,02,02), ccaPercent = 17.1 },
                      new CCAEntry{ date = new DateTime(2016,02,03), ccaPercent = 26.6 },
                      new CCAEntry{ date = new DateTime(2016,02,04), ccaPercent = 35.2 },
                      new CCAEntry{ date = new DateTime(2016,02,05), ccaPercent = 37.8 }
                    },
                  },
                                   new CCAData
                  {
                    machineName = "CAT23",
                    entries = new List<CCAEntry>
                    {
                      new CCAEntry{ date = new DateTime(2016,02,01), ccaPercent = 16.1 },
                      new CCAEntry{ date = new DateTime(2016,02,02), ccaPercent = 20.1 },
                      new CCAEntry{ date = new DateTime(2016,02,03), ccaPercent = 29.6 },
                      new CCAEntry{ date = new DateTime(2016,02,04), ccaPercent = 45.2 },
                      new CCAEntry{ date = new DateTime(2016,02,05), ccaPercent = 58.8 }
                    },
                  },
                                   new CCAData
                  {
                    machineName = "CAT51",
                    entries = new List<CCAEntry>
                    {
                      new CCAEntry{ date = new DateTime(2016,02,01), ccaPercent = 29.2 },
                      new CCAEntry{ date = new DateTime(2016,02,02), ccaPercent = 45.1 },
                      new CCAEntry{ date = new DateTime(2016,02,03), ccaPercent = 49.1 },
                      new CCAEntry{ date = new DateTime(2016,02,04), ccaPercent = 60.0 },
                      new CCAEntry{ date = new DateTime(2016,02,05), ccaPercent = 60.0 }
                    },
                  }        
                };
            return Ok(data);
          }
          catch (InvalidOperationException)
          {
            return Ok();
          }
        }
        #endregion
    }
}
