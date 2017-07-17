using System;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.KafkaConsumer.JsonConverters
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
      //if (jObject["FavoriteGeofenceEvent"] != null)
      //{
      //  return jObject["FavoriteGeofenceEvent"].ToObject<FavoriteGeofenceEvent>();
      //}
      //if (jObject["UnfavoriteGeofenceEvent"] != null)
      //{
      //  return jObject["UnfavoriteGeofenceEvent"].ToObject<UnfavoriteGeofenceEvent>();
      //}

      return null;
    }
  }
}
