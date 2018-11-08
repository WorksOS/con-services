using System;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.KafkaConsumer.JsonConverters
{
  public class AssetEventConverter : JsonCreationConverter<IAssetEvent>
  {
    protected override IAssetEvent Create(Type objectType, JObject jObject)
    {
      if (jObject["CreateAssetEvent"] != null)
      {
        return jObject["CreateAssetEvent"].ToObject<CreateAssetEvent>();
      }
      if (jObject["UpdateAssetEvent"] != null)
      {
        return jObject["UpdateAssetEvent"].ToObject<UpdateAssetEvent>();
      }
      if (jObject["DeleteAssetEvent"] != null)
      {
        return jObject["DeleteAssetEvent"].ToObject<DeleteAssetEvent>();
      }
      return null;
    }
  }
}