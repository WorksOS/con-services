using VLPDDecls;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Request DTO from 3DP to Raptor/TRex.
  /// </summary>
  public class LineworkRequest : ProjectID, IValidatable
  {
    public DesignDescriptor LineworkDescriptor { get; private set; }
    public TVLPDDistanceUnits LineworkUnits { get; private set; }
    public int MaxBoundariesToProcess { get; private set; }
    public int MaxVerticesPerBoundary { get; private set; }
    public string CoordSystemFileName { get; private set; }

    private LineworkRequest()
    { }

    public static LineworkRequest Create(
      long projectId,
      DesignDescriptor lineworkDescriptor,
      TVLPDDistanceUnits lineworkUnits,
      int maxBoundariesToProcess,
      int maxVerticiesPerBoundary,
      string coordSystemFilename)
    {
      var result = new LineworkRequest
      {
        ProjectId = projectId,
        LineworkDescriptor = lineworkDescriptor,
        CoordSystemFileName = coordSystemFilename,
        LineworkUnits = lineworkUnits,
        MaxBoundariesToProcess = maxBoundariesToProcess,
        MaxVerticesPerBoundary = maxVerticiesPerBoundary
      };

      result.Validate();

      return result;
    }

    public override void Validate()
    {
      base.Validate();

      throw new System.NotImplementedException();
    }
  }
}
