using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DBModels
{
  public class ProjectGeofence
  {
    public string ProjectUID { get; set; }
    public string GeofenceUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}
