using System;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.KafkaConsumer.JsonConverters
{
  public class FilterEventConverter : JsonCreationConverter<IFilterEvent>
  {
    protected override IFilterEvent Create(Type objectType, JObject jObject)
    {
      if (jObject["CreateFilterEvent"] != null)
      {
        return jObject["CreateFilterEvent"].ToObject<CreateFilterEvent>();
      }
      if (jObject["UpdateFilterEvent"] != null)
      {
        return jObject["UpdateFilterEvent"].ToObject<UpdateFilterEvent>();
      }
      if (jObject["DeleteFilterEvent"] != null)
      {
        return jObject["DeleteFilterEvent"].ToObject<DeleteFilterEvent>();
      }
      return null;
    }
  }
}
