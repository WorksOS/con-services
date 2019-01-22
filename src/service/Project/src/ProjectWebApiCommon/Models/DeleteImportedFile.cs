using System;
using System.IO;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  public class DeleteImportedFile
  {
    public Guid ProjectUid { get; set; }

    public long LegacyProjectId { get; set; }

    public ImportedFileType ImportedFileType { get; set; }

    public DateTime? SurveyedUtc { get; set; } 

    public FileDescriptor FileDescriptor { get; set; }

    public Guid ImportedFileUid { get; set; }

    public long ImportedFileId { get; set; }

    public long? LegacyImportedFileId { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private DeleteImportedFile()
    {
    }

    public static DeleteImportedFile CreateDeleteImportedFile(
      Guid projectUid, ImportedFileType importedFileTypeId,
      //DateTime? surveyedUtc, 
      FileDescriptor fileDescriptor, 
      Guid importedFileUid, long importedFileId, long? legacyImportedFileId
    )
    {
      return new DeleteImportedFile()
      {
        ProjectUid = projectUid,       
        ImportedFileType = importedFileTypeId,
        //SurveyedUtc = surveyedUtc,       
        FileDescriptor = fileDescriptor,
        ImportedFileUid = importedFileUid,
        ImportedFileId = importedFileId,
        LegacyImportedFileId = legacyImportedFileId
      };
    }

  }
}

