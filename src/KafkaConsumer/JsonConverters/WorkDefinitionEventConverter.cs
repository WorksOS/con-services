using System;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.UnifiedProductivity.Service.Utils.JsonConverters
{
  public class WorkDefinitionEventConverter : JsonCreationConverter<IWorkDefinitionEvent>
  {
    protected override IWorkDefinitionEvent Create(Type objectType, JObject jObject)
    {
      if (jObject["CreateWorkDefinitionEvent"] != null)
      {
        return jObject["CreateWorkDefinitionEvent"].ToObject<CreateWorkDefinitionEvent>(); 
      }
      if (jObject["UpdateWorkDefinitionEvent"] != null)
      {
        return jObject["UpdateWorkDefinitionEvent"].ToObject<UpdateWorkDefinitionEvent>();
      }
      return null;
    }

  }
}
