using System;
using VSS.MasterData.Models.Models;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// The INTERNAL request representation used to Create an importedFile. 
  /// </summary>
  public class CreateImportedFile : ImportedFileBase
  {
    public string FileName { get; set; }
    public DxfUnitsType DxfUnitsType { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }
    public Guid? ParentUid { get; set; }
    public double? Offset { get; set; }
    public Guid ImportedFileUid { get; set; }

    /// <summary>
    /// Create instance of CreateImportedFile
    /// </summary>
    public CreateImportedFile(Guid projectUid,
      string fileName, FileDescriptor fileDescriptor, ImportedFileType importedFileType,
      DateTime? surveyedUtc, DxfUnitsType dxfUnitsType, DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      string dataOceanRootFolder, Guid? parentUid, double? offset, Guid importedFileUid, string dataOceanFileName)
    {
      ProjectUid = projectUid;
      FileName = fileName;
      FileDescriptor = fileDescriptor;
      ImportedFileType = importedFileType;
      SurveyedUtc = surveyedUtc;
      DxfUnitsType = dxfUnitsType;
      FileCreatedUtc = fileCreatedUtc;
      FileUpdatedUtc = fileUpdatedUtc;
      DataOceanRootFolder = dataOceanRootFolder;
      ParentUid = parentUid;
      Offset = offset;
      ImportedFileUid = importedFileUid;
      DataOceanFileName = dataOceanFileName;
    }
  }
}
