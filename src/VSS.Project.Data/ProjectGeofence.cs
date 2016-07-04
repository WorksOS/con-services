using System;


namespace VSS.Project.Data
{
  public class ProjectGeofence
  {
    public string ProjectUID { get; set; }
    public string GeofenceUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}
