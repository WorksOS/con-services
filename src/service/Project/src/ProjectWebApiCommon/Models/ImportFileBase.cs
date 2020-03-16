using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public abstract class ImportedFileBase
  { 
    public string ProjectUid { get; set; }
    public ImportedFileType ImportedFileType { get; set; }
    public FileDescriptor FileDescriptor { get; set; }

    public DateTime? SurveyedUtc { get; set; }
    public string DataOceanRootFolder { get; set; }

    public string DataOceanFileName { get; set; }

    /// <summary>
    /// Cannot delete a design (or alignment) which is used in a filter
    /// </summary>
    /// <remarks>
    /// When scheduled reports are implemented, extend this check to them as well.
    /// </remarks>
    [JsonIgnore]
    public bool IsDesignFileType =>
      ImportedFileType == ImportedFileType.DesignSurface ||
      ImportedFileType == ImportedFileType.SurveyedSurface ||
      ImportedFileType == ImportedFileType.Alignment ||
      ImportedFileType == ImportedFileType.ReferenceSurface;
  }
}
