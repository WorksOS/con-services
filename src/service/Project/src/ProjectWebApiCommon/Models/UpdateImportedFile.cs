using System;
using VSS.MasterData.Models.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class UpdateImportedFile : ImportedFileBase
  {
    public long ShortRaptorProjectId { get; set; }
    public DxfUnitsType DxfUnitsTypeId { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }
    public Guid ImportedFileUid { get; set; }
    public long ImportedFileId { get; set; }
    public double? Offset { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public UpdateImportedFile(
      Guid projectUid, long shortRaptorProjectId, ImportedFileType importedFileTypeId,
      DateTime? surveyedUtc, DxfUnitsType dxfUnitsTypeId,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      FileDescriptor fileDescriptor, Guid importedFileUid, long importedFileId,
      string dataOceanRootFolder, double? offset, string dataOceanFileName)
    {
      ProjectUid = projectUid;
      ShortRaptorProjectId = shortRaptorProjectId;
      ImportedFileType = importedFileTypeId;
      SurveyedUtc = surveyedUtc;
      DxfUnitsTypeId = dxfUnitsTypeId;
      FileCreatedUtc = fileCreatedUtc;
      FileUpdatedUtc = fileUpdatedUtc;
      FileDescriptor = fileDescriptor;
      ImportedFileUid = importedFileUid;
      ImportedFileId = importedFileId;
      DataOceanRootFolder = dataOceanRootFolder;
      Offset = offset;
      DataOceanFileName = dataOceanFileName;
    }
  }
}
