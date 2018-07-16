using System;
using System.Collections.Generic;
using System.Linq;
using Common.netstandard.ApiClients;
using Common.Repository;
using log4net;
using LandfillService.Common.ApiClients;
using LandfillService.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;

namespace LandFillServiceDataSynchronizer
{
  public class DataSynchronizer
  {
    private const string userId = "sUpErSeCretIdTuSsupport348215890UnknownRa754291";
    private readonly ILog Log;
    private readonly I_3dpmAuthN authn;

    public DataSynchronizer(ILog logger)
    {
      Log = logger;
      authn = new _3dpmAuthN(new GenericConfiguration(new NullLoggerFactory()),
        new TPaasProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
        new Logger<_3dpmAuthN>(new NullLoggerFactory()));
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
      var headers =
        new Dictionary<string, string> {{"Authorization", $"Bearer {authn.Get3DPmSchedulerBearerToken().Result}"}};
      foreach (var project in projects)
      {
        headers["X-VisionLink-CustomerUID"] = project.customerUid;
        try
        {
          var startDate =
            new RaptorApiClient(new NullLoggerFactory().CreateLogger(""),
                new GenericConfiguration(new NullLoggerFactory()),
                new RaptorProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
                new FileListProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory(),
                  new MemoryCache(new MemoryCacheOptions())), headers).GetProjectStatisticsAsync(userId, project).Result
              .startTime.Date;
          if (startDate < DateTime.Today.AddDays(-90))
            startDate = DateTime.Today.AddDays(-90);
          var geofenceUids = LandfillDb.GetGeofences(project.projectUid).Select(g => g.uid.ToString()).ToList();

          result.Add(project, geofenceUids.SelectMany(g => Enumerable
            .Range(0, 1 + DateTime.Today.Subtract(startDate).Days)
            .Select(offset => startDate.AddDays(offset))
            .Select(d => new DateEntry { geofenceUid = g, date = d })).ToList());
        }
        catch (Exception e)
        {
          Console.WriteLine($"Skipping project {project.name} as failed to get statistics");
          //Log.Debug($"Skipping project {project.name} as failed to get statistics");
        }
      }
      Console.WriteLine("Got {0} entries to process for volumes", result.Count);
      //Log.DebugFormat("Got {0} entries to process for volumes", result.Count);

      return result;
    }

    private Dictionary<string, List<WGSPoint>> GetGeofenceBoundaries(uint id, List<string> geofenceUids)
    {
      var geofences = geofenceUids.ToDictionary(g => g,
        g => LandfillDb.GetGeofencePoints(g).ToList());
      Log.DebugFormat("Got {0} geofences to process for projectResponse {1}", geofenceUids.Count, id);

      return geofences;
    }

    public void RunUpdateVolumesFromRaptor()
    {
      var datesToUpdate = GetListOfEntriesToUpdate();

      foreach (var project in datesToUpdate)
      {
        var geofenceUids = project.Value.Select(d => d.geofenceUid).Distinct().ToList();
        var geofences = GetGeofenceBoundaries(project.Key.id, geofenceUids);
        var headers =
          new Dictionary<string, string> { { "Authorization", $"Bearer {authn.Get3DPmSchedulerBearerToken().Result}" } };
        Log.DebugFormat("RunUpdateVolumesFromRaptor Processing project {0} with {1} entries", project.Key.id, project.Value.Count());
        foreach (var dateEntry in project.Value)
        {
          headers["X-VisionLink-CustomerUID"] = project.Key.customerUid;
          var geofence = geofences.ContainsKey(dateEntry.geofenceUid) ? geofences[dateEntry.geofenceUid] : null;
          new RaptorApiClient(new NullLoggerFactory().CreateLogger(""),
              new GenericConfiguration(new NullLoggerFactory()),
              new RaptorProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
              new FileListProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory(),
                new MemoryCache(new MemoryCacheOptions())), headers)
                    .GetVolumeInBackground(userId, project.Key, geofence, dateEntry).Wait();
        }
      }
    }

    public void RunUpdateCCAFromRaptor()
    {
      //1. Do the scheduled date for each projectResponse (note: UTC date)
      //2. Do missing dates with no CCA for each projectResponse (note: these are projectResponse time zone)
      //3. Retry unretrieved entries for each projectResponse (also projectResponse time zone)


      //Use same criteria as volumes to select projects to process. 
      //No point in getting CCA if no weights or volumes and therefore no density data.
      var projects = GetListOfProjectsToRetrieve();
      Log.DebugFormat("RunUpdateCCAFromRaptor Got {0} projects to process for CCA", projects.Count);
      var headers =
        new Dictionary<string, string> { { "Authorization", $"Bearer {authn.Get3DPmSchedulerBearerToken().Result}" } };

      foreach (var project in projects)
      {
        //   if (projectResponse.id != 2712) continue;

        var utcDate = DateTime.UtcNow.AddMonths(-1);
        utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
        Log.InfoFormat("RunUpdateCCAFromRaptor Processing projectID {0} name {1} timezone {2}", project.id, project.name, project.legacyTimeZoneName);

        var geofenceUids = LandfillDb.GetGeofences(project.projectUid).Select(g => g.uid.ToString()).ToList();
        var geofences = GetGeofenceBoundaries(project.id, geofenceUids);
        headers["X-VisionLink-CustomerUID"] = project.customerUid;
        //Process CCA for scheduled date
        //var hwZone =
        //  new RaptorApiClient(new NullLoggerFactory().CreateLogger(""),
        //    new GenericConfiguration(new NullLoggerFactory()),
        //    new RaptorProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
        //    new FileListProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory(),
        //      new MemoryCache(new MemoryCacheOptions())), headers).GetTimeZoneInfoForTzdbId(project.timeZoneName);
        //var projDate = utcDate.Date.Add(hwZone.BaseUtcOffset);
        //var nowDate = DateTime.UtcNow.Date.Add(hwZone.BaseUtcOffset);

        var offsetMinutes = new RaptorApiClient(new NullLoggerFactory().CreateLogger(""),
            new GenericConfiguration(new NullLoggerFactory()),
            new RaptorProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
            new FileListProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory(),
              new MemoryCache(new MemoryCacheOptions())), headers).ConvertFromTimeZoneToMinutesOffset(project.legacyTimeZoneName);
        Log.DebugFormat("RunUpdateCCAFromRaptor Timezone {0} with minutes offset {1} ", project.legacyTimeZoneName, offsetMinutes);
        var projDate = utcDate.Date.AddMinutes(offsetMinutes);
        var nowDate = DateTime.UtcNow.Date.AddMinutes(offsetMinutes);
        
        //In case we're backfilling...
        while (projDate <= nowDate)
        {
          var machinesToProcess =
            new RaptorApiClient(new NullLoggerFactory().CreateLogger(""),
                new GenericConfiguration(new NullLoggerFactory()),
                new RaptorProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
                new FileListProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory(),
                  new MemoryCache(new MemoryCacheOptions())), headers)
              .GetMachineLiftsInBackground(userId, project, utcDate.Date, utcDate.Date).Result;
          Log.DebugFormat("RunUpdateCCAFromRaptor Processing projectResponse {0} with {1} machines for date {2}", project.id,machinesToProcess.Count,utcDate.Date);
          ProcessCCA(utcDate.Date, project, geofenceUids, geofences, machinesToProcess);
          utcDate = utcDate.Date.AddDays(1);
          projDate = projDate.Date.AddDays(1);
        }
      }
    }

    /// <summary>
    ///   Process CCA for the projectResponse and date.
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
      var headers =
        new Dictionary<string, string> { { "Authorization", $"Bearer {authn.Get3DPmSchedulerBearerToken().Result}" } };
      headers["X-VisionLink-CustomerUID"] = projectResponse.customerUid;

      foreach (var geofenceUid in geofenceUids)
      {
        var geofence = geofences.ContainsKey(geofenceUid) ? geofences[geofenceUid] : null;

        foreach (var machine in machines)
        {
          foreach (var lift in machine.lifts)
          {
            Log.DebugFormat("ProcessCCA machine lifts {0}, geofence {1}, machine {2}, lift {3}, machineId {4}",
              projectResponse.id, geofenceUid, machine, lift.layerId, machineIds[machine]);
            new RaptorApiClient(new NullLoggerFactory().CreateLogger(""),
              new GenericConfiguration(new NullLoggerFactory()),
              new RaptorProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
              new FileListProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory(),
                new MemoryCache(new MemoryCacheOptions())), headers).GetCCAInBackground(
              userId, projectResponse, geofenceUid, geofence, date, machineIds[machine], machine, lift.layerId).Wait();
          }

          //Also do the 'All Lifts'
          Log.DebugFormat("ProcessCCA all lifts {0}, geofence {1}, machine {2}, lift {3}, machineId {4}",
            projectResponse.id, geofenceUid, machine, "ALL", machineIds[machine]);
          new RaptorApiClient(new NullLoggerFactory().CreateLogger(""),
            new GenericConfiguration(new NullLoggerFactory()),
            new RaptorProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
            new FileListProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory(),
              new MemoryCache(new MemoryCacheOptions())), headers).GetCCAInBackground(
            userId, projectResponse, geofenceUid, geofence, date, machineIds[machine], machine, null).Wait();
        }
      }
    }
  }
}