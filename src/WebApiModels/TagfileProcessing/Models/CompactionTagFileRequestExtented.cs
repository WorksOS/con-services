using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models
{
  /// <summary>
  /// A helper so that we can have both the TRex and Raptor
  ///    paths in the same executor
  /// </summary>
  public class CompactionTagFileRequestExtended : CompactionTagFileRequest
  {
    public long? ProjectId { get; private set; }
    
    /// <summary>
    /// The boundary of the project to process the TAG file into. If the location of the data in the TAG file is outside of this boundary it will not be processed into the project.
    /// May be null.
    /// </summary>
    [JsonProperty]
    public WGS84Fence Boundary { get; private set; }
    
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
        long projectId,
        WGS84Fence boundary)
    {
      return new CompactionTagFileRequestExtended
      {
        FileName = compactionTagFileRequest.FileName,
        Data = compactionTagFileRequest.Data,
        OrgId = compactionTagFileRequest.OrgId,
        ProjectId = projectId,
        Boundary = boundary
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
