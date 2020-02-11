using System;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class UpdateImportedFile : ImportedFileBase
  {
    public long LegacyProjectId { get; set; }
    public DxfUnitsType DxfUnitsTypeId { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }
    public Guid ImportedFileUid { get; set; }
    public long ImportedFileId { get; set; }
    public double? Offset { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// uploadToTCC parameter is used for the TCC to DataOcean migration; when this is completed the parameter could be removed.
    /// It's presence here is to prevent calling into Raptor on file upsert and not generate extra files, e.g. .prg.
    /// </remarks>
    public UpdateImportedFile(
      Guid projectUid, long legacyProjectId, ImportedFileType importedFileTypeId,
      DateTime? surveyedUtc, DxfUnitsType dxfUnitsTypeId,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      FileDescriptor fileDescriptor, Guid importedFileUid, long importedFileId,
      string dataOceanRootFolder, double? offset, string dataOceanFileName)
    {
      ProjectUid = projectUid;
      LegacyProjectId = legacyProjectId;
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
