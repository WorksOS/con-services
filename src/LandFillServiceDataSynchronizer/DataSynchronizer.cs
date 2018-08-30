using System;
using System.Collections.Generic;
using System.Linq;
using Common.Repository;
using LandfillService.Common.ApiClients;
using LandfillService.Common.Models;
using log4net;

namespace LandFillServiceDataSynchronizer
{
  public class DataSynchronizer
  {
    private const string userId = "sUpErSeCretIdTuSsupport348215890UnknownRa754291";
    private ILog Log;

    public DataSynchronizer(ILog logger)
    {
      Log = logger;
    }

    //private RaptorApiClient raptorApiClient = new RaptorApiClient();

    private List<ProjectResponse> GetListOfProjectsToRetrieve()
    {
      return LandfillDb.GetListOfAvailableProjects();
    }

    private Dictionary<ProjectResponse, List<DateEntry>> GetListOfEntriesToUpdate()
    {
      var projects = GetListOfProjectsToRetrieve();
      var result = new Dictionary<ProjectResponse, List<DateEntry>>();
      Log.DebugFormat("Got {0} projects to process for volumes", projects.Count);
      foreach (var project in projects)
      {
        var startDate = new RaptorApiClient().GetProjectStatisticsAsync(userId, project).Result.startTime.Date;
        if (startDate < DateTime.Today.AddDays(-90))
          startDate = DateTime.Today.AddDays(-90);
        var geofenceUids = LandfillDb.GetGeofences(project.projectUid).Select(g => g.uid.ToString()).ToList();

        result.Add(project, geofenceUids.SelectMany(g => Enumerable
          .Range(0, 1 + DateTime.Today.Subtract(startDate).Days)
          .Select(offset => startDate.AddDays(offset))
          .Select(d => new DateEntry() {geofenceUid = g, date = d})).ToList());
      }

/*      Dictionary<Project, List<DateEntry>> entries = projects.ToDictionary(projectResponse => projectResponse,
          projectResponse => LandfillDb.GetDatesWithVolumesNotRetrieved(projectResponse).ToList());*/
      Log.DebugFormat("Got {0} entries to process for volumes", result.Count);

      return result;
    }

    private Dictionary<string, List<WGSPoint>> GetGeofenceBoundaries(uint id, List<string> geofenceUids)
    {
      Dictionary<string, List<WGSPoint>> geofences = geofenceUids.ToDictionary(g => g,
        g => LandfillDb.GetGeofencePoints(g).ToList());
      Log.DebugFormat("Got {0} geofences to process for projectResponse {1}", geofenceUids.Count, id);

      return geofences;
    }

    public void RunUpdateVolumesFromRaptor(object state)
    {
      var datesToUpdate = GetListOfEntriesToUpdate();

      foreach (var project in datesToUpdate)
      {
        var geofenceUids = project.Value.Select(d => d.geofenceUid).Distinct().ToList();
        var geofences = GetGeofenceBoundaries(project.Key.id, geofenceUids);

        Log.DebugFormat("Processing projectResponse {0} with {1} entries", project.Key.id, project.Value.Count());
        foreach (var dateEntry in project.Value)
        {
          var geofence = geofences.ContainsKey(dateEntry.geofenceUid) ? geofences[dateEntry.geofenceUid] : null;
          new RaptorApiClient().GetVolumeInBackground(userId, project.Key, geofence, dateEntry).Wait();
        }
      }

    }

    public void RunUpdateCCAFromRaptor(object state)
    {
      //1. Do the scheduled date for each projectResponse (note: UTC date)
      //2. Do missing dates with no CCA for each projectResponse (note: these are projectResponse time zone)
      //3. Retry unretrieved entries for each projectResponse (also projectResponse time zone)



      //Use same criteria as volumes to select projects to process. 
      //No point in getting CCA if no weights or volumes and therefore no density data.
      var projects = GetListOfProjectsToRetrieve();
      Log.DebugFormat("Got {0} projects to process for CCA", projects.Count);

      foreach (var project in projects)
      {
        //   if (projectResponse.id != 2712) continue;

        var utcDate = (DateTime) state;
        utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
        Log.InfoFormat("START Processing projectResponse {0}", project.id);

        var geofenceUids = LandfillDb.GetGeofences(project.projectUid).Select(g => g.uid.ToString()).ToList();
        var geofences = GetGeofenceBoundaries(project.id, geofenceUids);

        //Process CCA for scheduled date
        TimeZoneInfo hwZone = new RaptorApiClient().GetTimeZoneInfoForTzdbId(project.timeZoneName);
        var projDate = utcDate.Date.Add(hwZone.BaseUtcOffset);
        var nowDate = DateTime.UtcNow.Date.Add(hwZone.BaseUtcOffset);
        //In case we're backfilling...
        while (projDate <= nowDate)
        {
          var machinesToProcess =
            new RaptorApiClient().GetMachineLiftsInBackground(userId, project, utcDate.Date, utcDate.Date).Result;
          Log.DebugFormat("Processing projectResponse {0} with {1} machines for date {2}", project.id, machinesToProcess.Count,
            utcDate.Date);

          ProcessCCA(utcDate.Date, project, geofenceUids, geofences, machinesToProcess);
          utcDate = utcDate.Date.AddDays(1);
          projDate = projDate.Date.AddDays(1);
        }

      }

    }

    /// <summary>
    /// Process CCA for the projectResponse and date.
    /// </summary>
    /// <param name="date">Date (in projectResponse time zone)</param>
    /// <param name="projectResponse">Project</param>
    /// <param name="geofenceUids">GeofenceResponse UIDs</param>
    /// <param name="geofences">GeofenceResponse boundaries</param>
    /// <param name="machines">Machines and lifts to process for given date</param>
    private void ProcessCCA(DateTime date, ProjectResponse projectResponse, IEnumerable<string> geofenceUids,
      Dictionary<string, List<WGSPoint>> geofences, IEnumerable<MachineLifts> machines)
    {
      var machineIds = machines.ToDictionary(m => m, m => LandfillDb.GetMachineId(projectResponse.projectUid, m));

      foreach (var geofenceUid in geofenceUids)
      {
        var geofence = geofences.ContainsKey(geofenceUid) ? geofences[geofenceUid] : null;

        foreach (var machine in machines)
        {
          foreach (var lift in machine.lifts)
          {
            Log.DebugFormat("Processing projectResponse {0}, geofence {1}, machine {2}, lift {3}, machineId {4}",
              projectResponse.id, geofenceUid, machine, lift.layerId, machineIds[machine]);
            new RaptorApiClient().GetCCAInBackground(
              userId, projectResponse, geofenceUid, geofence, date, machineIds[machine], machine, lift.layerId).Wait();
          }
          //Also do the 'All Lifts'
          Log.DebugFormat("Processing projectResponse {0}, geofence {1}, machine {2}, lift {3}, machineId {4}",
            projectResponse.id, geofenceUid, machine, "ALL", machineIds[machine]);
          new RaptorApiClient().GetCCAInBackground(
            userId, projectResponse, geofenceUid, geofence, date, machineIds[machine], machine, null).Wait();
        }
      }
    }

  }
}
