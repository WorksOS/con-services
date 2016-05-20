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
      return null;
    }
  }
}
