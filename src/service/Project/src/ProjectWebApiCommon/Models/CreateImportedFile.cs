using System;
using System.IO;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// The INTERNAL request representation used to Create an importedFile. 
  /// </summary>
  public class CreateImportedFile
  {
    public Guid ProjectUid { get; set; }

    public string FileName { get; set; }

    public FileDescriptor FileDescriptor { get; set; }

    public ImportedFileType ImportedFileType { get; set; }

    public DateTime? SurveyedUtc { get; set; }

    public DxfUnitsType DxfUnitsType { get; set; }

    public DateTime FileCreatedUtc { get; set; }

    public DateTime FileUpdatedUtc { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CreateImportedFile()
    { }

    /// <summary>
    /// Create instance of CreateImportedFile
    /// </summary>
    public static CreateImportedFile CreateACreateImportedFile(Guid projectUid,
      string fileName, FileDescriptor fileDescriptor, ImportedFileType importedFileType, 
      DateTime? surveyedUtc, DxfUnitsType dxfUnitsType, DateTime fileCreatedUtc, DateTime fileUpdatedUtc )
    {
      return new CreateImportedFile
      {
        ProjectUid = projectUid,
        FileName = fileName,
        FileDescriptor = fileDescriptor,
        ImportedFileType = importedFileType,
        SurveyedUtc = surveyedUtc,
        DxfUnitsType = dxfUnitsType,
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc
      };
    }
  }
}
