using System;
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSS.Landfill.Common.JsonConverters
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
      return null;
    }
  }
}
