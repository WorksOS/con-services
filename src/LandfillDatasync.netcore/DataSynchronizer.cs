using System;
using System.Collections.Generic;
using System.Linq;
using Common.netstandard.ApiClients;
using Common.Repository;
using log4net;
using LandfillService.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;

namespace LandfillDatasync.netcore
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

    private List<Project> GetListOfProjectsToRetrieve()
    {
      return LandfillDb.GetListOfAvailableProjects();
    }

    /// <summary>
    /// Get list of volume entries from raptor 
    /// </summary>
    /// <param name="noOfDaysVols"></param>
    /// <returns></returns>
    private Dictionary<Project, List<DateEntry>> GetListOfEntriesToUpdate(int noOfDaysVols)
    {
      var projects = GetListOfProjectsToRetrieve();
      var result = new Dictionary<Project, List<DateEntry>>();
      Log.DebugFormat("Got {0} projects to process for volumes", projects.Count);
      var headers = new Dictionary<string, string> {{"Authorization", $"Bearer {authn.Get3DPmSchedulerBearerToken().Result}"}};
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
          if (startDate < DateTime.Today.AddDays(noOfDaysVols))
            startDate = DateTime.Today.AddDays(noOfDaysVols);
          var geofenceUids = LandfillDb.GetGeofences(project.projectUid).Select(g => g.uid.ToString()).ToList();

          result.Add(project, geofenceUids.SelectMany(g => Enumerable
            .Range(0, 1 + DateTime.Today.Subtract(startDate).Days)
            .Select(offset => startDate.AddDays(offset))
            .Select(d => new DateEntry { geofenceUid = g, date = d })).ToList());
          Log.DebugFormat($"Valid project {project.name} has {geofenceUids.Count} geofences");
        }
        catch (Exception e)
        {
          Log.DebugFormat("Skipping project {0} as failed. Exception: {1}",project.name,e.Message);
        }
      }
      Log.DebugFormat("Got {0} entries to process for volumes", result.Count);
      return result;
    }

    private Dictionary<string, List<WGSPoint>> GetGeofenceBoundaries(uint id, List<string> geofenceUids)
    {
      var geofences = geofenceUids.ToDictionary(g => g,
        g => LandfillDb.GetGeofencePoints(g).ToList());
      Log.DebugFormat("Got {0} geofences to process for projectID {1}", geofenceUids.Count, id);

      return geofences;
    }

    public void RunUpdateVolumesFromRaptor(int noOfDaysVols)
    {
      Log.DebugFormat("***** Start Processing volumes for the last {0} days",noOfDaysVols);
      var datesToUpdate = GetListOfEntriesToUpdate(noOfDaysVols);

      foreach (var project in datesToUpdate)
      {
        var geofenceUids = project.Value.Select(d => d.geofenceUid).Distinct().ToList();
        var geofences = GetGeofenceBoundaries(project.Key.id, geofenceUids);
        var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {authn.Get3DPmSchedulerBearerToken().Result}" } };
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

    /// <summary>
    /// Call raptor every day to get CCA 
    /// </summary>
    /// <param name="ccaDaysBackFill"></param>
    public void RunUpdateCcaFromRaptor(int ccaDaysBackFill)
    {
      //1. Do the scheduled date for each project (note: UTC date)
      //2. Do missing dates with no CCA for each project (note: these are project time zone)
      //3. Retry unretrieved entries for each project (also project time zone)

      //Use same criteria as volumes to select projects to process. 
      //No point in getting CCA if no weights or volumes and therefore no density data.
      Log.InfoFormat("***** Start Processing CCA for the last {0} days", ccaDaysBackFill);
      var projects = GetListOfProjectsToRetrieve();
      Log.InfoFormat("UpdateCCA: Got {0} projects to process for CCA", projects.Count);
      var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {authn.Get3DPmSchedulerBearerToken().Result}" } };

      foreach (var project in projects)
      {
        //   if (project.id != 2712) continue;
        try
        {
          //var utcDate = DateTime.UtcNow.AddMonths(-1);
          var utcDate = DateTime.UtcNow.AddDays(ccaDaysBackFill); 
          utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
          Log.InfoFormat("UpdateCCA: Get Geofences ProjectID {0} name {1} timezone {2}", project.id, project.name, project.timeZoneName);
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
                new MemoryCache(new MemoryCacheOptions())), headers).ConvertFromTimeZoneToMinutesOffset(project.timeZoneName);
          Log.InfoFormat("UpdateCCA: Processing projectID {0} name {1} timezone {2} with minutes offset {3}", project.id, project.name, project.timeZoneName, offsetMinutes);
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
            Log.DebugFormat("UpdateCCA: ProcessCCA projectId {0} with {1} machines for date {2}", project.id,machinesToProcess.Count,utcDate.Date);
            ProcessCCA(utcDate.Date, project, geofenceUids, geofences, machinesToProcess);
            utcDate = utcDate.Date.AddDays(1);
            projDate = projDate.Date.AddDays(1);
          }

        }
        catch (Exception e)
        {
          Log.WarnFormat("UpdateCCA: Skipping project {0} id {1} - Exception {2}", project.name,project.id, e.Message);
        }
      }
    }

    /// <summary>
    ///   Process CCA for the project and date.
    /// </summary>
    /// <param name="date">Date (in project time zone)</param>
    /// <param name="project">Project</param>
    /// <param name="geofenceUids">GeofenceResponse UIDs</param>
    /// <param name="geofences">GeofenceResponse boundaries</param>
    /// <param name="machines">Machines and lifts to process for given date</param>
    private void ProcessCCA(DateTime date, Project project, IEnumerable<string> geofenceUids,
      Dictionary<string, List<WGSPoint>> geofences, IEnumerable<MachineLifts> machines)
    {
      var machineIds = machines.ToDictionary(m => m, m => LandfillDb.GetMachineId(project.projectUid, m));
      var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {authn.Get3DPmSchedulerBearerToken().Result}" } };
      headers["X-VisionLink-CustomerUID"] = project.customerUid;

      foreach (var geofenceUid in geofenceUids)
      {
        var geofence = geofences.ContainsKey(geofenceUid) ? geofences[geofenceUid] : null;

        foreach (var machine in machines)
        {
          foreach (var lift in machine.lifts)
          {
           // Log.DebugFormat("ProcessCCA machine lifts {0}, geofence {1}, machine {2}, lift {3}, machineId {4}",project.id, geofenceUid, machine, lift.layerId, machineIds[machine]);
            new RaptorApiClient(new NullLoggerFactory().CreateLogger(""),
              new GenericConfiguration(new NullLoggerFactory()),
              new RaptorProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
              new FileListProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory(),
                new MemoryCache(new MemoryCacheOptions())), headers).GetCCAInBackground(
              userId, project, geofenceUid, geofence, date, machineIds[machine], machine, lift.layerId).Wait();
          }

          //Also do the 'All Lifts'
          //Log.DebugFormat("ProcessCCA all lifts {0}, geofence {1}, machine {2}, lift {3}, machineId {4}",project.id, geofenceUid, machine, "ALL", machineIds[machine]);
          new RaptorApiClient(new NullLoggerFactory().CreateLogger(""),
            new GenericConfiguration(new NullLoggerFactory()),
            new RaptorProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory()),
            new FileListProxy(new GenericConfiguration(new NullLoggerFactory()), new NullLoggerFactory(),
              new MemoryCache(new MemoryCacheOptions())), headers).GetCCAInBackground(
            userId, project, geofenceUid, geofence, date, machineIds[machine], machine, null).Wait();
        }
      }
    }
  }
}