using System;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.KafkaConsumer.JsonConverters
{
  public class ProjectEventConverter : JsonCreationConverter<IProjectEvent>
  {
    protected override IProjectEvent Create(Type objectType, JObject jObject)
    {
      if (jObject["CreateProjectEvent"] != null)
      {
        return jObject["CreateProjectEvent"].ToObject<CreateProjectEvent>();
      }
      if (jObject["UpdateProjectEvent"] != null)
      {
        return jObject["UpdateProjectEvent"].ToObject<UpdateProjectEvent>();
      }
      if (jObject["DeleteProjectEvent"] != null)
      {
        return jObject["DeleteProjectEvent"].ToObject<DeleteProjectEvent>();
      }
      if (jObject["AssociateProjectCustomer"] != null)
      {
        return jObject["AssociateProjectCustomer"].ToObject<AssociateProjectCustomer>();
      }
      if (jObject["AssociateProjectGeofence"] != null)
      {
        return jObject["AssociateProjectGeofence"].ToObject<AssociateProjectGeofence>();
      }

      //if (jObject["DissociateProjectCustomer"] != null)
      //{
      //  return jObject["DissociateProjectCustomer"].ToObject<DissociateProjectCustomer>();
      //}
      //if (jObject["RestoreProjectEvent"] != null)
      //{
      //  return jObject["RestoreProjectEvent"].ToObject<RestoreProjectEvent>();
      //}
      return null;
    }
  }
}

