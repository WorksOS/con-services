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
      Log.DebugFormat("Got {0} projects to process",projects.Count);
      Dictionary<Project, List<DateEntry>> entries = projects.ToDictionary(project => project,
          project => LandfillDb.GetDatesWithVolumesNotRetrieved(project).ToList());
      Log.DebugFormat("Got {0} entries to process", entries.Count);

      return entries;
    }

    private Dictionary<string, List<WGSPoint>> GetGeofenceBoundaries(uint id, List<string> geofenceUids)
    {
      Dictionary<string, List<WGSPoint>> geofences = geofenceUids.ToDictionary(g => g,
          g => LandfillDb.GetGeofencePoints(g).ToList());
      Log.DebugFormat("Got {0} geofences to process for project {1}", geofenceUids.Count, id);

      return geofences;
    }

    public void RunUpdateDataFromRaptor(object state)
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
          GetVolumeInBackground("sUpErSeCretIdTuSsupport348215890UnknownRa754291", project.Key, geofence, dateEntry).Wait();
        }
      }

    }

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
        Log.DebugFormat("Get volume for project {0} date {1}", project.id,entry.date);

        var res = await raptorApiClient.GetVolumesAsync(userUid, project, entry.date, geofence);

        Log.Debug("Volume res:" + res);

        Log.Debug("Volume: " + (res.Fill));

        LandfillDb.SaveVolume(project.id, entry.geofenceUid, entry.date, res.Fill);
      }
      catch (RaptorApiException e)
      {
        if (e.code == HttpStatusCode.BadRequest)
        {
          // this response code is returned when the volume isn't available (e.g. the time range
          // is outside project extents); the assumption is that's the only reason we will
          // receive a 400 Bad Request 

          Log.Warn("RaptorApiException while retrieving volumes: " + e.Message);
          LandfillDb.MarkVolumeNotAvailable(project.id, entry.geofenceUid, entry.date);

          // TESTING CODE
          // Volume range in m3 should be ~ [478, 1020]
          //LandfillDb.SaveVolume(project.id, entry.date, new Random().Next(541) + 478, entry.geofenceUid);
        }
        else
        {
          Log.Error("RaptorApiException while retrieving volumes: " + e.Message);
          LandfillDb.MarkVolumeNotRetrieved(project.id, entry.geofenceUid, entry.date);
        }
      }
      catch (Exception e)
      {
        Log.Error("Exception while retrieving volumes: " + e.Message);
        LandfillDb.MarkVolumeNotRetrieved(project.id, entry.geofenceUid, entry.date);
      }
    }


  }
}
