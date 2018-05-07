using System;
using VSS.MasterData.Models.Models;
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

    public FileDescriptor FileDescriptor { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ImportedFileUpsertEvent()
    {
    }

    public static ImportedFileUpsertEvent CreateImportedFileUpsertEvent(Repositories.DBModels.Project project,
      ImportedFileType importedFileTypeId,
      DateTime? surveyedUtc, DxfUnitsType dxfUnitsTypeId, DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      FileDescriptor fileDescriptor
    )
    {
      return new ImportedFileUpsertEvent()
      {
        Project = project,
        ImportedFileTypeId = importedFileTypeId,
        SurveyedUtc = surveyedUtc,
        DxfUnitsTypeId = dxfUnitsTypeId,
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        FileDescriptor = fileDescriptor
      };
    }

  }
}

