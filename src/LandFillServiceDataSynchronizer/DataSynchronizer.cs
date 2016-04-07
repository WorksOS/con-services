using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;
using LandfillService.Common;
using LandfillService.WebApi.ApiClients;
using LandfillService.WebApi.Models;

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

    private Dictionary<Project, List<DateTime>> GetListOfEntriesToUpdate()
    {
      var projects = GetListOfProjectsToRetrieve();
      Log.DebugFormat("Got {0} projects to process",projects.Count);
      Dictionary<Project, List<DateTime>> entries = projects.ToDictionary(project => project,
          project => LandfillDb.GetDatesWithVolumesNotRetrieved(project.id).ToList());
      Log.DebugFormat("Got {0} entries to process", entries.Count);

      return entries;
    }

    public void RunUpdateDataFromRaptor(object state)
    {
      var datesToUpdate = GetListOfEntriesToUpdate();

      foreach (var project in datesToUpdate)
      {
        var entries = project.Value.Select(date => new WeightEntry { date = date, weight = 0 }); // generate fake WeightEntry objects from dates
        Log.DebugFormat("Processing project {0} with {1} entries", project.Key.id, entries.Count());        
        foreach (var weightEntry in entries)
        {
          GetVolumeInBackground("sUpErSeCretIdTuSsupport348215890UnknownRa754291", project.Key, weightEntry).Wait();
        }
      }

    }

    /// <summary>
    /// Retrieves volume summary from Raptor and saves it to the landfill DB
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">Project</param>
    /// <param name="entry">Weight entry from the client</param>
    /// <returns></returns>
    private async Task GetVolumeInBackground(string userUid, Project project, WeightEntry entry)
    {
      try
      {
        Log.DebugFormat("Get volume for project {0} date {1}", project.id,entry.date);

        var res = await raptorApiClient.GetVolumesAsync(userUid,project, entry.date);

        Log.Debug("Volume res:" + res);

        Log.Debug("Volume: " + (res.Fill));

        LandfillDb.SaveVolume(project.id, entry.date, res.Fill);
      }
      catch (RaptorApiException e)
      {
        if (e.code == HttpStatusCode.BadRequest)
        {
          // this response code is returned when the volume isn't available (e.g. the time range
          // is outside project extents); the assumption is that's the only reason we will
          // receive a 400 Bad Request 

          Log.Warn("RaptorApiException while retrieving volumes: " + e.Message);
          LandfillDb.MarkVolumeNotAvailable(project.id, entry.date);

          // TESTING CODE
          // Volume range in m3 should be ~ [478, 1020]
          //LandfillDb.SaveVolume(project.id, entry.date, new Random().Next(541) + 478);
        }
        else
        {
          Log.Error("RaptorApiException while retrieving volumes: " + e.Message);
          LandfillDb.MarkVolumeNotRetrieved(project.id, entry.date);
        }
      }
      catch (Exception e)
      {
        Log.Error("Exception while retrieving volumes: " + e.Message);
        LandfillDb.MarkVolumeNotRetrieved(project.id, entry.date);
      }
    }


  }
}
