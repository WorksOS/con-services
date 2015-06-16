using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using LandfillService.WebApi.Models;
using LandfillService.WebApi.ApiClients;
using System.Collections;
using System.Web.Hosting;
using LandfillService.Common.Contracts;
using LandfillService.Common;
using NodaTime;
using System.Reflection;

namespace LandfillService.WebApi.Controllers
{
    [RoutePrefix("api/v1/projects")]
    public class ProjectsController : ApiController
    {
        private ForemanApiClient foremanApiClient = new ForemanApiClient();
        private RaptorApiClient raptorApiClient = new RaptorApiClient();

        private IHttpActionResult ForemanRequest(string sessionId, Func<IHttpActionResult> body)
        {
            try
            {
                return body();
            }
            catch (ForemanApiException e)
            {
                if (e.code == HttpStatusCode.Unauthorized)
                    LandfillDb.DeleteSession(sessionId);
                return Content(e.code, e.Message);
            }
        }


        private IEither<IHttpActionResult, IEnumerable<Project>> GetProjects(string sessionId)
        {
            try
            {
                var projects = foremanApiClient.GetProjects(sessionId);
                System.Diagnostics.Debug.WriteLine(projects);
                LandfillDb.SaveProjects(sessionId, projects);
                return Either.Right<IHttpActionResult, IEnumerable<Project>>(projects);
            }
            catch (ForemanApiException e)
            {
                if (e.code == HttpStatusCode.Unauthorized)
                    LandfillDb.DeleteSession(sessionId);
                return Either.Left<IHttpActionResult, IEnumerable<Project>>(Content(e.code, e.Message));
            }
        }

        private IEither<IHttpActionResult, IEnumerable<Project>>  PerhapsUpdateProjectList(string sessionId)
        {
            if (LandfillDb.GetProjectListAgeInHours(sessionId) < 1)
                return Either.Right<IHttpActionResult, IEnumerable<Project>>(LandfillDb.GetProjects(sessionId));

            return GetProjects(sessionId);
        }

        /// <summary>
        /// Returns the list of projects avaialable to the user.
        /// </summary>
        /// <param name="request">Session ID</param>
        /// <returns>List of available projects</returns>
        [Route("")]
        public IHttpActionResult Get()
        {
            var sessionId = Request.Headers.GetValues("SessionId").First();

            return PerhapsUpdateProjectList(sessionId).Case(errorResponse => errorResponse, projects => Ok(projects));
        }

        private IEnumerable<DayEntry> GetRandomEntries()
        {
            var totalDays = 730;
            var startDate = DateTime.Today.AddDays(-totalDays);

            var entries = new List<DayEntry>();

            //if (id == 544)
            //    return entries.ToArray();

            var rnd = new Random();

            var densityExtra = rnd.Next(1, 3);
            var weightExtra = rnd.Next(200, 300);


            foreach (int i in Enumerable.Range(0, totalDays))
            {
                bool skip = (i < 728 && rnd.Next(5) % 6 == 0);

                entries.Add(new DayEntry
                {
                    date = DateTime.Today.AddDays(-totalDays + i),
                    entryPresent = !skip,
                    density = skip ? 0 : rnd.Next(1200 / densityExtra, 1600 / densityExtra),
                    weight = skip ? 0 : rnd.Next(500, 800 + weightExtra)
                });
            }
            return entries.ToArray(); 

        }

        /// <summary>
        /// Returns the last two years worth of project data for a given project.
        /// </summary>
        /// <param name="request">Project ID, session ID</param>
        /// <returns>List of data entries for each day in the last two years</returns>
        [Route("{id}")]
        public IHttpActionResult Get(uint id)
        {
            // Get the available data
            // Kick off missing volumes retrieval IF not already running
            // Check if there are missing volumes and indicate to the client


            var sessionId = Request.Headers.GetValues("SessionId").First();

            return PerhapsUpdateProjectList(sessionId).Case(errorResponse => errorResponse, projects => 
            {
                try
                {
                    var project = projects.Where(p => p.id == id).First();
                    GetMissingVolumesInBackground(sessionId, project);  // retry volume requests which weren't successful before

                    //GetRandomEntries()
                    return Ok(new ProjectData { entries = LandfillDb.GetEntries(project), retrievingVolumes = LandfillDb.RetrievalInProgress(project) });
                }
                catch (InvalidOperationException)
                {
                    return Ok();
                }
            });

        }


        private async Task GetVolumeInBackground(string sessionId, Project project, WeightEntry entry)
        {
            try
            {
                var res = await raptorApiClient.GetVolumesAsync(sessionId, project, entry.date);

                System.Diagnostics.Debug.WriteLine("Volume res:" + res);
                System.Diagnostics.Debug.WriteLine("Volume: " + (res.Fill - res.Cut));

                LandfillDb.SaveVolume(project.id, entry.date, res.Fill - res.Cut);
            }
            catch (RaptorApiException e)
            {
                if (e.code == HttpStatusCode.BadRequest)
                {
                    // this response code is returned when the volume isn't available (e.g. the time range
                    // is outside project extents); the assumption is that's the only reason we will
                    // receive a 400 Bad Request 
                    System.Diagnostics.Debug.Write("RaptorApiException while retrieving volumes: " + e);
                    LandfillDb.MarkVolumeNotAvailable(project.id, entry.date);

                    // TESTING CODE
                    // Volume range in m3 should be ~ [478, 1020]
                    //LandfillDb.SaveVolume(project.id, entry.date, new Random().Next(541) + 478);
                }
                else
                    LandfillDb.MarkVolumeNotRetrieved(project.id, entry.date);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write("Exception while retrieving volumes: " + e);
                LandfillDb.MarkVolumeNotRetrieved(project.id, entry.date);
                throw;
            }
        }

        private void GetMissingVolumesInBackground(string sessionId, Project project)
        {
            // get a "lock" on the project so that only a single background task at a time is retrieving 
            // missing volumes 
            var noRetrievalInProgress = LandfillDb.LockForRetrieval(project);

            if (noRetrievalInProgress)
            {
                var dates = LandfillDb.GetDatesWithVolumesNotRetrieved(project.id);
                System.Diagnostics.Debug.Write("Dates without volumes: {0}", dates.ToString());
                var entries = dates.Select(date => new WeightEntry { date = date, weight = 0 });
                GetVolumesInBackground(sessionId, project, entries, () =>
                {
                    var retrievalWasInProgress = LandfillDb.LockForRetrieval(project, false);
                    if (!retrievalWasInProgress)
                        LoggerSvc.LogMessage(GetType().Name, MethodBase.GetCurrentMethod().Name, "Project id: " + project.id.ToString(),
                            "Project wasn't locked for retrieval when it should have been");
                });
            }
            else
                System.Diagnostics.Debug.Write("Retrieval of missing volumes already in progress");
        }

        private void GetVolumesInBackground(string sessionId, Project project, IEnumerable<WeightEntry> entries, Action onComplete)
        {
            HostingEnvironment.QueueBackgroundWorkItem(async (CancellationToken cancel) =>
            {
                const int parallelRequestCount = 10;

                for (var offset = 0; offset <= entries.Count() / parallelRequestCount; offset++)
                {
                    var tasks = entries.Skip(offset * parallelRequestCount).Take(parallelRequestCount).Select(entry => GetVolumeInBackground(sessionId, project, entry));
                    await Task.WhenAll(tasks);
                }

                onComplete();
            });
        }

        /// <summary>
        /// Saves weights submitted in the request.
        /// </summary>
        /// <param name="request">Project ID, Session ID, array of weight entries</param>
        /// <returns>Project data and status of volume retrieval</returns>
        [Route("{id}/weights")]
        public IHttpActionResult PostWeights(uint id, [FromBody] WeightEntry[] entries)
        {
            // TODO: Get project list and check request validity
            var sessionId = Request.Headers.GetValues("SessionId").First();

            return PerhapsUpdateProjectList(sessionId).Case(errorResponse => errorResponse, projects =>
            {
                var project = projects.Where(p => p.id == id).First();


                var validEntries = new List<WeightEntry>();
                foreach (var entry in entries)
                {
                    System.Diagnostics.Debug.WriteLine(entry.ToString());

                    var projTimeZone = DateTimeZoneProviders.Tzdb[project.timeZoneName];
                    var dateInProjTimeZone = projTimeZone.AtLeniently(new LocalDateTime(entry.date.Year, entry.date.Month, entry.date.Day, 0, 0));
                    var utcDateTime = dateInProjTimeZone.ToDateTimeUtc();

                    //   There doesn't seem to be a standard way to convert a time from an arbitrary time zone to UTC 
                    //   This was my best attempt which didn't work:
                    //TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZone.IanaToWindows(project.timeZoneName));
                    //var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(
                    //    new DateTime(entry.date.Year, entry.date.Month, entry.date.Day, 0, 0, 0, 0, DateTimeKind.Local), timeZone);

                    if (entry.weight >= 0 && utcDateTime <= DateTime.Today.AddDays(-1).ToUniversalTime())
                    {
                        LandfillDb.SaveEntry(id, entry);
                        validEntries.Add(entry);
                    }
                };

                GetVolumesInBackground(sessionId, project, validEntries, () =>
                {
                    GetMissingVolumesInBackground(sessionId, project);
                });

                System.Diagnostics.Debug.WriteLine("Finished posting weights");

                //throw new ServiceException(HttpStatusCode.BadGateway, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "ERROR!!!"));
                //throw new InvalidOperationException("UH OH");
                return Ok(new ProjectData { entries = LandfillDb.GetEntries(project), retrievingVolumes = true });

            });
        }
    }
}
