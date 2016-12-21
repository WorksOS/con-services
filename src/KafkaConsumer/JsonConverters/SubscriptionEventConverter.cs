using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VSS.UnifiedProductivity.Service.Utils.JsonConverters;
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
            return jObject["UpdateCustomerSubscriptionEvent"]?.ToObject<UpdateCustomerSubscriptionEvent>();
        }
    }
}
