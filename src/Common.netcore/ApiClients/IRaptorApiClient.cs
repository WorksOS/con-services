using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LandfillService.Common.Models;

namespace LandfillService.Common.ApiClients
{
  public interface IRaptorApiClient
  {
    Task<SummaryVolumesResult> GetAirspaceVolumeAsync(string userUid, ProjectResponse projectResponse,
      bool returnEarliest, int designId);

    Task GetCCAInBackground(string userUid, ProjectResponse projectResponse, string geofenceUid,
      List<WGSPoint> geofence, DateTime date, long machineId, MachineDetails machine, int? liftId);

    Task<List<DesignDescriptiorLegacy>> GetDesignID(string jwt, ProjectResponse projectResponse, string customerUid);

    Task<List<MachineLifts>> GetMachineLiftsInBackground(string userUid, ProjectResponse projectResponse,
      DateTime startDate, DateTime endDate);

    Task<ProjectStatisticsResult> GetProjectStatisticsAsync(string userUid, ProjectResponse projectResponse);
    TimeZoneInfo GetTimeZoneInfoForTzdbId(string tzdbId);

    Task GetVolumeInBackground(string userUid, ProjectResponse projectResponse, List<WGSPoint> geofence,
      DateEntry entry);
  }
}