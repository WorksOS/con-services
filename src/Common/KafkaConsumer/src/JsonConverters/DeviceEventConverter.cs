using System;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.KafkaConsumer.JsonConverters
{
  public class DeviceEventConverter : JsonCreationConverter<IDeviceEvent>
  {
    protected override IDeviceEvent Create(Type objectType, JObject jObject)
    {
      if (jObject["CreateDeviceEvent"] != null)
      {
        return jObject["CreateDeviceEvent"].ToObject<CreateDeviceEvent>();
      }
      if (jObject["UpdateDeviceEvent"] != null)
      {
        return jObject["UpdateDeviceEvent"].ToObject<UpdateDeviceEvent>();
      }   
      if (jObject["AssociateDeviceAssetEvent"] != null)
      {
        return jObject["AssociateDeviceAssetEvent"].ToObject<AssociateDeviceAssetEvent>();
      }
      if (jObject["DissociateDeviceAssetEvent"] != null)
      {
        return jObject["DissociateDeviceAssetEvent"].ToObject<DissociateDeviceAssetEvent>();
      }

      return null;
    }
  }
}