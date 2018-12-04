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
    public string CoordSystemFileName { get; private set; }
    public int NumberOfBoundariesToProcess => string.IsNullOrEmpty(CoordSystemFileName) ? 1 : __Global.MAX_BOUNDARIES_TO_PROCESS;
    public int NumberOfVerticesPerBoundary = __Global.MAX_VERTICES_PER_BOUNDARY;

    private LineworkRequest()
    { }

    public static LineworkRequest Create(
      long projectId,
      DesignDescriptor lineworkDescriptor,
      TVLPDDistanceUnits lineworkUnits,
      string coordSystemFilename)
    {
      var result = new LineworkRequest
      {
        ProjectId = projectId,
        LineworkDescriptor = lineworkDescriptor,
        CoordSystemFileName = coordSystemFilename?.Trim(),
        LineworkUnits = lineworkUnits,
      };

      result.Validate();

      return result;
    }

    public override void Validate()
    {
      base.Validate();

   //   throw new System.NotImplementedException();
    }
  }
}
