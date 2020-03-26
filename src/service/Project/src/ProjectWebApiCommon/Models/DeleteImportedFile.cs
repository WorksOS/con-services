using System;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  public class DeleteImportedFile : ImportedFileBase
  {
    public string ImportedFileUid { get; set; }
    public long ImportedFileId { get; set; }
    public long? LegacyImportedFileId { get; set; }

    public DeleteImportedFile(
      string projectUid, ImportedFileType importedFileTypeId,
      FileDescriptor fileDescriptor, string importedFileUid, long importedFileId,
      long? legacyImportedFileId, string dataOceanRootFolder, DateTime? surveyedUtc)
    {
      ProjectUid = projectUid;
      ImportedFileType = importedFileTypeId;
      FileDescriptor = fileDescriptor;
      ImportedFileUid = importedFileUid;
      ImportedFileId = importedFileId;
      LegacyImportedFileId = legacyImportedFileId;
      DataOceanRootFolder = dataOceanRootFolder;
      SurveyedUtc = surveyedUtc;
    }
  }
}

