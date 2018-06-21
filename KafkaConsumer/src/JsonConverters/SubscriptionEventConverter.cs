using System;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.KafkaConsumer.JsonConverters
{
  public class SubscriptionEventConverter : JsonCreationConverter<ISubscriptionEvent>
  {
    protected override ISubscriptionEvent Create(Type objectType, JObject jObject)
    {
      if (jObject["CreateProjectSubscriptionEvent"] != null)
      {
        return jObject["CreateProjectSubscriptionEvent"].ToObject<CreateProjectSubscriptionEvent>();
      }
      if (jObject["UpdateProjectSubscriptionEvent"] != null)
      {
        return jObject["UpdateProjectSubscriptionEvent"].ToObject<UpdateProjectSubscriptionEvent>();
      }
      if (jObject["AssociateProjectSubscriptionEvent"] != null)
      {
        return jObject["AssociateProjectSubscriptionEvent"].ToObject<AssociateProjectSubscriptionEvent>();
      }
      if (jObject["DissociateProjectSubscriptionEvent"] != null)
      {
        return jObject["DissociateProjectSubscriptionEvent"].ToObject<DissociateProjectSubscriptionEvent>();
      }
      if (jObject["CreateCustomerSubscriptionEvent"] != null)
      {
        return jObject["CreateCustomerSubscriptionEvent"].ToObject<CreateCustomerSubscriptionEvent>();
      }
      if (jObject["UpdateCustomerSubscriptionEvent"] != null)
      {
        return jObject["UpdateCustomerSubscriptionEvent"]?.ToObject<UpdateCustomerSubscriptionEvent>();
      }
      if (jObject["CreateAssetSubscriptionEvent"] != null)
      {
        return jObject["CreateAssetSubscriptionEvent"].ToObject<CreateAssetSubscriptionEvent>();
      }
      if (jObject["UpdateAssetSubscriptionEvent"] != null)
      {
        return jObject["UpdateAssetSubscriptionEvent"]?.ToObject<UpdateAssetSubscriptionEvent>();
      }

      return null;
    }
  }
}