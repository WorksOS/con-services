using System.Net;
using Microsoft.AspNetCore.Http;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Request DTO from 3DP to Raptor/TRex.
  /// </summary>
  public class LineworkRequest : ProjectID
  {
    /// <summary>
    /// Gets the temporary working directory on the Raptor IONode host.
    /// </summary>
    public const string FILE_PATH = @"D:\VLPDProductionData\Temp\12121212\"; // TODO Change to something tied to the Canary project id.

    public string Filename { get; private set; }
    public IFormFile FileData { get; private set; }
    public string FilespaceId { get; private set; }
    public TVLPDDistanceUnits LineworkUnits { get; private set; }
    public string CoordSystemFileName { get; private set; }
    public FileDescriptor FileDescriptor { get; private set; }
    
    public int NumberOfBoundariesToProcess => string.IsNullOrEmpty(CoordSystemFileName) ? 1 : __Global.MAX_BOUNDARIES_TO_PROCESS;
    public int NumberOfVerticesPerBoundary = __Global.MAX_VERTICES_PER_BOUNDARY;

    private LineworkRequest()
    { }

    public static LineworkRequest Create(DxfFileRequest fileRequest)
    {
      // Raptor's Canary project is consistent across deployed environments and acts as a suitable placeholder here.
      const int raptorCanaryProjectId = 987654321;

      var result = new LineworkRequest
      {
        ProjectId = raptorCanaryProjectId,
        FileDescriptor = FileDescriptor.CreateFileDescriptor(fileRequest.FilespaceId, FILE_PATH, fileRequest.Filename),
        CoordSystemFileName = fileRequest.CoordinateSystemName?.Trim(),
        LineworkUnits = (TVLPDDistanceUnits)fileRequest.DistanceUnits,
        Filename = fileRequest.Filename,
        FileData = fileRequest.FileData,
        FilespaceId = fileRequest.FilespaceId
      };

      return result;
    }

    public new LineworkRequest Validate()
    {
      // Don't need to validate project Id or UID on this request object. By design we don't have them.

      if (FileData == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "File data cannot be null"));
      }

      // TODO (Aaron) Complete validation, filename, linework units, etc.

      return this;
    }
  }
}
