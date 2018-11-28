using System;
using System.IO;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  public class UpdateImportedFile
  {
    public Guid ProjectUid { get; set; }
    public long LegacyProjectId { get; set; }

    public ImportedFileType ImportedFileType { get; set; }

    public DateTime? SurveyedUtc { get; set; }

    public DxfUnitsType DxfUnitsTypeId { get; set; }

    public DateTime FileCreatedUtc { get; set; }

    public DateTime FileUpdatedUtc { get; set; }

    public FileDescriptor FileDescriptor { get; set; }

    public Guid ImportedFileUid { get; set; }

    public long ImportedFileId { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private UpdateImportedFile()
    {
    }

    public static UpdateImportedFile CreateUpdateImportedFile(
      Guid projectUid, long legacyProjectId, ImportedFileType importedFileTypeId,
      DateTime? surveyedUtc, DxfUnitsType dxfUnitsTypeId, 
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      FileDescriptor fileDescriptor, Guid importedFileUid, long importedFileId
    )
    {
      return new UpdateImportedFile()
      {
        ProjectUid = projectUid,
        LegacyProjectId = legacyProjectId,
        ImportedFileType = importedFileTypeId,
        SurveyedUtc = surveyedUtc,
        DxfUnitsTypeId = dxfUnitsTypeId,
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        FileDescriptor = fileDescriptor,
        ImportedFileUid = importedFileUid,
        ImportedFileId = importedFileId
      };
    }

  }
}

