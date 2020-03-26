using System;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
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
      if (jObject["CreateImportedFileEvent"] != null)
      {
        return jObject["CreateImportedFileEvent"].ToObject<CreateImportedFileEvent>();
      }
      if (jObject["UpdateImportedFileEvent"] != null)
      {
        return jObject["UpdateImportedFileEvent"].ToObject<UpdateImportedFileEvent>();
      }
      if (jObject["DeleteImportedFileEvent"] != null)
      {
        return jObject["DeleteImportedFileEvent"].ToObject<DeleteImportedFileEvent>();
      }
      if (jObject["UndeleteImportedFileEvent"] != null)
      {
        return jObject["UndeleteImportedFileEvent"].ToObject<UndeleteImportedFileEvent>();
      }
      if (jObject["UpdateProjectSettingsEvent"] != null)
      {
        return jObject["UpdateProjectSettingsEvent"].ToObject<UpdateProjectSettingsEvent>();
      }

      // RestoreProjectEvent is there for rollback only and will never be put on the kafka que
      //if (jObject["RestoreProjectEvent"] != null)
      //{
      //  return jObject["RestoreProjectEvent"].ToObject<RestoreProjectEvent>();
      //}
      return null;
    }
  }
}

