using System;
using VSS.MasterData.Models.Models;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  public class DeleteImportedFile : ImportedFileBase
  {
    public Guid ImportedFileUid { get; set; }
    public long ImportedFileId { get; set; }
    public long? LegacyImportedFileId { get; set; }

    public DeleteImportedFile(
      Guid projectUid, ImportedFileType importedFileTypeId,
      FileDescriptor fileDescriptor, Guid importedFileUid, long importedFileId,
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

