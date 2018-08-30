using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LandfillService.Common.Models;

namespace LandfillService.Common.ApiClients
{
  public interface IRaptorApiClient
  {
    Task<SummaryVolumesResult> GetAirspaceVolumeAsync(string userUid, Project project,
      bool returnEarliest, int designId);

    Task GetCCAInBackground(string userUid, Project project, string geofenceUid,
      List<WGSPoint> geofence, DateTime date, long machineId, MachineDetails machine, int? liftId);

    Task<List<DesignDescriptiorLegacy>> GetDesignID(string jwt, Project project, string customerUid);

    Task<List<MachineLifts>> GetMachineLiftsInBackground(string userUid, Project project,
      DateTime startDate, DateTime endDate);

    Task<ProjectStatisticsResult> GetProjectStatisticsAsync(string userUid, Project project);
    TimeZoneInfo GetTimeZoneInfoForTzdbId(string tzdbId);

    Task GetVolumeInBackground(string userUid, Project project, List<WGSPoint> geofence,
      DateEntry entry);
  }
}