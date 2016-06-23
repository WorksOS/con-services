using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LandfillService.Common.ApiClients;
using LandfillService.Common.Context;
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

    private RaptorApiClient raptorApiClient = new RaptorApiClient();

    private List<Project> GetListOfProjectsToRetrieve()
    {
      return LandfillDb.GetListOfAvailableProjects();
    }

    private Dictionary<Project, List<DateEntry>> GetListOfEntriesToUpdate()
    {
      var projects = GetListOfProjectsToRetrieve();
      Log.DebugFormat("Got {0} projects to process for volumes",projects.Count);
      Dictionary<Project, List<DateEntry>> entries = projects.ToDictionary(project => project,
          project => LandfillDb.GetDatesWithVolumesNotRetrieved(project).ToList());
      Log.DebugFormat("Got {0} entries to process for volumes", entries.Count);

      return entries;
    }

    private Dictionary<string, List<WGSPoint>> GetGeofenceBoundaries(uint id, List<string> geofenceUids)
    {
      Dictionary<string, List<WGSPoint>> geofences = geofenceUids.ToDictionary(g => g,
          g => LandfillDb.GetGeofencePoints(g).ToList());
      Log.DebugFormat("Got {0} geofences to process for project {1}", geofenceUids.Count, id);

      return geofences;
    }

    public void RunUpdateVolumesFromRaptor(object state)
    {
      var datesToUpdate = GetListOfEntriesToUpdate();

      foreach (var project in datesToUpdate)
      {
        var geofenceUids = project.Value.Select(d => d.geofenceUid).Distinct().ToList();
        var geofences = GetGeofenceBoundaries(project.Key.id, geofenceUids);

        Log.DebugFormat("Processing project {0} with {1} entries", project.Key.id, project.Value.Count());        
        foreach (var dateEntry in project.Value)
        {
          var geofence = geofences.ContainsKey(dateEntry.geofenceUid) ? geofences[dateEntry.geofenceUid] : null;
          raptorApiClient.GetVolumeInBackground(userId, project.Key, geofence, dateEntry).Wait();
        }
      }

    }

    public void RunUpdateCCAFromRaptor(object state)
    {
      //1. Do the scheduled date for each project (note: UTC date)
      //2. Do missing dates with no CCA for each project (note: these are project time zone)
      //3. Retry unretrieved entries for each project (also project time zone)

      var utcDate = (DateTime) state;

      //Use same criteria as volumes to select projects to process. 
      //No point in getting CCA if no weights or volumes and therefore no density data.
      var projects = GetListOfProjectsToRetrieve();
      Log.DebugFormat("Got {0} projects to process for CCA", projects.Count);

      foreach (var project in projects)
      {
        Log.InfoFormat("START Processing project {0}", project.id);

        var geofenceUids = LandfillDb.GetGeofences(project.projectUid).Select(g => g.uid.ToString()).ToList();
        var geofences = GetGeofenceBoundaries(project.id, geofenceUids);

        //Process CCA for scheduled date
        TimeZoneInfo hwZone = raptorApiClient.GetTimeZoneInfoForTzdbId(project.timeZoneName);
        var projDate = utcDate.Date.Add(hwZone.BaseUtcOffset);

        var machinesToProcess = raptorApiClient.GetMachineLiftsInBackground(userId, project, projDate, projDate).Result;
        Log.DebugFormat("Processing project {0} with {1} machines for date {2}", project.id, machinesToProcess.Count, projDate);

        ProcessCCA(projDate, project, geofenceUids, geofences, machinesToProcess);

        //Process CCA for missing dates
        var missingDates = LandfillDb.GetDatesWithNoCCA(project);
        foreach (var missingDate in missingDates)
        {
          machinesToProcess = raptorApiClient.GetMachineLiftsInBackground(userId, project, missingDate, missingDate).Result;
          Log.DebugFormat("Processing project {0} with {1} machines for missing date {2}", project.id, machinesToProcess.Count, missingDate);

          ProcessCCA(projDate, project, geofenceUids, geofences, machinesToProcess);          
        }

        //Process unretrieved ones
        var notRetrieved = LandfillDb.GetEntriesWithCCANotRetrieved(project).ToList();
        var machineIds = notRetrieved.Select(nr => nr.machineId).Distinct();
        var machines = machineIds.ToDictionary(m => m, m => LandfillDb.GetMachine(m));
        foreach (var cca in notRetrieved)
        {
          var geofence = geofences.ContainsKey(cca.geofenceUid) ? geofences[cca.geofenceUid] : null;

          Log.DebugFormat("Re-processing project {0}, geofence {1}, machine {2}, lift {3}", project.id, cca.geofenceUid, machines[cca.machineId], cca.liftId);
          raptorApiClient.GetCCAInBackground(
            userId, project, cca.geofenceUid, geofence, cca.date, cca.machineId, machines[cca.machineId], cca.liftId).Wait();          
        }
        Log.InfoFormat("END Processing project {0}", project.id);
      }

    }

     /// <summary>
     /// Process CCA for the project and date.
     /// </summary>
     /// <param name="date">Date (in project time zone)</param>
     /// <param name="project">Project</param>
     /// <param name="geofenceUids">Geofence UIDs</param>
     /// <param name="geofences">Geofence boundaries</param>
     /// <param name="machines">Machines and lifts to process for given date</param>
    private void ProcessCCA(DateTime date, Project project, IEnumerable<string> geofenceUids, Dictionary<string, List<WGSPoint>> geofences, IEnumerable<MachineLiftDetails> machines)
    {
      var machineIds = machines.ToDictionary(m => m, m => LandfillDb.GetMachineId(m));

      foreach (var geofenceUid in geofenceUids)
      {
        var geofence = geofences.ContainsKey(geofenceUid) ? geofences[geofenceUid] : null;

        foreach (var machine in machines)
        {
          foreach (var lift in machine.lifts)
          {
            Log.DebugFormat("Processing project {0}, geofence {1}, machine {2}, lift {3}", project.id, geofenceUid, machine, lift.layerId);
            raptorApiClient.GetCCAInBackground(
              userId, project, geofenceUid, geofence, date, machineIds[machine], machine, lift.layerId).Wait();
          }
          //Also do the 'All Lifts'
          Log.DebugFormat("Processing project {0}, geofence {1}, machine {2}, lift {3}", project.id, geofenceUid, machine, "ALL");
          raptorApiClient.GetCCAInBackground(
            userId, project, geofenceUid, geofence, date, machineIds[machine], machine, null).Wait();
        }
      }
    }

  }
}
