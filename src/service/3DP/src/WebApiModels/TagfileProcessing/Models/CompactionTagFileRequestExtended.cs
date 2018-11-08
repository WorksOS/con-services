using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models
{
  /// <summary>
  /// A helper so that we can have both the TRex and Raptor
  ///    paths in the same executor
  /// </summary>
  public class CompactionTagFileRequestExtended : CompactionTagFileRequest
  {
    public WGS84Fence Boundary { get; private set; }

    public bool includeTrexIfConfigured { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionTagFileRequestExtended()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionTagFileRequestExtended CreateCompactionTagFileRequestExtended(
      CompactionTagFileRequest compactionTagFileRequest,
        WGS84Fence boundary,
        bool includeTrexIfConfigured = true)
    {
      return new CompactionTagFileRequestExtended
      {
        FileName = compactionTagFileRequest.FileName,
        Data = compactionTagFileRequest.Data,
        OrgId = compactionTagFileRequest.OrgId,
        ProjectId = compactionTagFileRequest.ProjectId,
        Boundary = boundary,
        includeTrexIfConfigured = includeTrexIfConfigured
      };
    }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionTagFileRequestExtended CreateCompactionTagFileRequestExtended(
      TagFileRequestLegacy tagFileRequestLegacy,
      long projectId,
      WGS84Fence boundary)
    {
      return new CompactionTagFileRequestExtended
      {
        FileName = tagFileRequestLegacy.FileName,
        Data = tagFileRequestLegacy.Data,
        OrgId = tagFileRequestLegacy.TccOrgId,
        ProjectId = projectId,
        Boundary = boundary
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      base.Validate();
    }
  }
}
