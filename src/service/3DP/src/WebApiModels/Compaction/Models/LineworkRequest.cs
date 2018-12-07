using VLPDDecls;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Request DTO from 3DP to Raptor/TRex.
  /// </summary>
  public class LineworkRequest : ProjectID
  {
    public FileDescriptor FileDescriptor { get;private set; }
    public TVLPDDistanceUnits LineworkUnits { get; private set; }
    public string CoordSystemFileName { get; private set; }
    public int NumberOfBoundariesToProcess => string.IsNullOrEmpty(CoordSystemFileName) ? 1 : __Global.MAX_BOUNDARIES_TO_PROCESS;
    public int NumberOfVerticesPerBoundary = __Global.MAX_VERTICES_PER_BOUNDARY;

    private LineworkRequest()
    { }

    public static LineworkRequest Create(
      string filename,
      string path,
      string filespaceId,
      TVLPDDistanceUnits lineworkUnits,
      string coordSystemFilename)
    {
      var result = new LineworkRequest
      {
        ProjectId = 1000544, // TODO Why is this required and why does it need to match... the folder name?
        FileDescriptor = FileDescriptor.CreateFileDescriptor(filespaceId, path, filename),
        CoordSystemFileName = coordSystemFilename?.Trim(),
        LineworkUnits = lineworkUnits
      };

      result.Validate();

      return result;
    }
  }
}
