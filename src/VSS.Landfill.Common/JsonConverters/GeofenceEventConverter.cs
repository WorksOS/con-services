using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Landfill.Common.JsonConverters
{
  public class GeofenceEventConverter : JsonCreationConverter<IGeofenceEvent>
  {
    protected override IGeofenceEvent Create(Type objectType, JObject jObject)
    {
      if (jObject["CreateGeofenceEvent"] != null)
      {
        return jObject["CreateGeofenceEvent"].ToObject<CreateGeofenceEvent>();
      }
      if (jObject["UpdateGeofenceEvent"] != null)
      {
        return jObject["UpdateGeofenceEvent"].ToObject<UpdateGeofenceEvent>();
      }
      if (jObject["DeleteGeofenceEvent"] != null)
      {
        return jObject["DeleteGeofenceEvent"].ToObject<DeleteGeofenceEvent>();
      }
      return null;
    }
  }
}
