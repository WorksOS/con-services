using System;
using Newtonsoft.Json.Linq;
using VSS.Project.Service.Utils.JsonConverters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace KafkaConsumer.JsonConverters
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
      if (jObject["CreateCustomerSubscriptionEvent"] != null)
      {
        return jObject["CreateCustomerSubscriptionEvent"].ToObject<CreateCustomerSubscriptionEvent>();
      }
      if (jObject["UpdateCustomerSubscriptionEvent"] != null)
      {
        return jObject["UpdateCustomerSubscriptionEvent"]?.ToObject<UpdateCustomerSubscriptionEvent>();
      }

      return null;
    }
  }
}