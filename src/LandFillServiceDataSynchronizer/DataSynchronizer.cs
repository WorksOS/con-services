using System;
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

    private Dictionary<Project, List<MachineLiftDetails>> GetListOfMachinesToProcess(DateTime utcDate)
    {
      //Use same criteria as volumes to select projects to process. 
      //No point in getting CCA if no weights or volumes and therefoe density data.
      var projects = GetListOfProjectsToRetrieve();
      Log.DebugFormat("Got {0} projects to process for CCA", projects.Count);
      Dictionary<Project, List<MachineLiftDetails>> machines = projects.ToDictionary(project => project,
          project => raptorApiClient.GetMachineLiftList(userId, project, utcDate).Result.ToList());
      Log.DebugFormat("Got {0} machines to process for CCA", machines.Count);

      return machines;
       
    }
     
    public void RunUpdateCCAFromRaptor(object state)
    {
      var utcDate = (DateTime) state;
      var machinesToProcess = GetListOfMachinesToProcess(utcDate);

      foreach (var project in machinesToProcess)
      {
        Log.DebugFormat("START Processing project {0} with {1} machines", project.Key.id, project.Value.Count);

        var geofenceUids = LandfillDb.GetGeofences(project.Key.id).Select(g => g.uid.ToString()).ToList();
        var geofences = GetGeofenceBoundaries(project.Key.id, geofenceUids);

        var machineIds = project.Value.ToDictionary(m => m, m => LandfillDb.GetMachineId(m));

        foreach (var geofenceUid in geofenceUids)
        {
          var geofence = geofences.ContainsKey(geofenceUid) ? geofences[geofenceUid] : null;

          foreach (var machine in project.Value)
          {
            foreach (var lift in machine.lifts)
            {
              Log.DebugFormat("Processing project {0}, geofence {1}, machine {2}, lift {3}", project.Key.id, geofenceUid, machine, lift.layerId);
              raptorApiClient.GetCCAInBackground(
                userId, project.Key, geofenceUid, geofence, utcDate, machineIds[machine], machine, lift.layerId).Wait();
            }
            //Also do the 'All Lifts'
            raptorApiClient.GetCCAInBackground(
              userId, project.Key, geofenceUid, geofence, utcDate, machineIds[machine], machine, null).Wait();
          }
        }
        Log.DebugFormat("END Processing project {0}", project.Key.id);
      }

    }

  }
}
