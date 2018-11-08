using VSS.MasterData.Repositories.DBModels;

namespace VSS.MasterData.Repositories.Extensions
{
  public static class GeofenceExtensions
  {
    /// <summary>
    /// Sets up a <see cref="Geofence"/> object so that if the Create comes later the fact that this is deleted is not lost.
    /// </summary>
    public static Geofence Setup(this Geofence geofence)
    {
      geofence.Name = "";
      geofence.GeofenceType = GeofenceType.Generic;
      geofence.GeometryWKT = "";
      geofence.FillColor = 0;
      geofence.IsTransparent = true;
      geofence.IsDeleted = true;
      geofence.Description = "";
      geofence.CustomerUID = "";
      geofence.UserUID = "";
      geofence.AreaSqMeters = 0;

      return geofence;
    }
  }
}