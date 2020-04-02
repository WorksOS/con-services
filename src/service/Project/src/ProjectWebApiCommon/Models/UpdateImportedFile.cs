using System;
using VSS.MasterData.Models.Models;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

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
    /// <remarks>
    /// uploadToTCC parameter is used for the TCC to DataOcean migration; when this is completed the parameter could be removed.
    /// It's presence here is to prevent calling into Raptor on file upsert and not generate extra files, e.g. .prg.
    /// </remarks>
    public UpdateImportedFile(
      string projectUid, long shortRaptorProjectId, ImportedFileType importedFileTypeId,
      DateTime? surveyedUtc, DxfUnitsType dxfUnitsTypeId,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      FileDescriptor fileDescriptor, string importedFileUid, long importedFileId,
      string dataOceanRootFolder, double? offset, string dataOceanFileName)
    {
      ProjectUid = new Guid(projectUid);
      ShortRaptorProjectId = shortRaptorProjectId;
      ImportedFileType = importedFileTypeId;
      SurveyedUtc = surveyedUtc;
      DxfUnitsTypeId = dxfUnitsTypeId;
      FileCreatedUtc = fileCreatedUtc;
      FileUpdatedUtc = fileUpdatedUtc;
      FileDescriptor = fileDescriptor;
      ImportedFileUid = new Guid(importedFileUid);
      ImportedFileId = importedFileId;
      DataOceanRootFolder = dataOceanRootFolder;
      Offset = offset;
      DataOceanFileName = dataOceanFileName;
    }
  }
}
