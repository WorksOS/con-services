using System;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.KafkaConsumer.JsonConverters
{
  public class CustomerEventConverter : JsonCreationConverter<ICustomerEvent>
  {
    protected override ICustomerEvent Create(Type objectType, JObject jObject)
    {
      if (jObject["CreateCustomerEvent"] != null)
      {
        return jObject["CreateCustomerEvent"].ToObject<CreateCustomerEvent>();
      }
      if (jObject["UpdateCustomerEvent"] != null)
      {
        return jObject["UpdateCustomerEvent"].ToObject<UpdateCustomerEvent>();
      }
      if (jObject["DeleteCustomerEvent"] != null)
      {
        return jObject["DeleteCustomerEvent"].ToObject<DeleteCustomerEvent>();
      }
      if (jObject["AssociateCustomerUserEvent"] != null)
      {
        return jObject["AssociateCustomerUserEvent"].ToObject<AssociateCustomerUserEvent>();
      }
      if (jObject["DissociateCustomerUserEvent"] != null)
      {
        return jObject["DissociateCustomerUserEvent"].ToObject<DissociateCustomerUserEvent>();
      }

      return null;
    }
  }
}