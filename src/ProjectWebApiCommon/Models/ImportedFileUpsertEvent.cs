using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  public class ImportedFileUpsertEvent 
  {
    public Repositories.DBModels.Project Project { get; set; }

    public ImportedFileType ImportedFileTypeId { get; set; }

    public DateTime? SurveyedUtc { get; set; }

    public DxfUnitsType DxfUnitsTypeId { get; set; }

    public DateTime FileCreatedUtc { get; set; }

    public DateTime FileUpdatedUtc { get; set; }

    public FileDescriptor ImportedFileInTcc { get; set; }
  }
}

