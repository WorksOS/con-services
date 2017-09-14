﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public interface IGeofenceRepository
  {
    Task<IEnumerable<Geofence>> GetCustomerGeofences(string customerUid);
    Task<Geofence> GetGeofence(string geofenceUid);
    Task<IEnumerable<Geofence>> GetProjectGeofencesByProjectUID(string projectUid);
    Task<int> StoreEvent(IGeofenceEvent evt);
  }
}