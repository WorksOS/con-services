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
    /// Project Id internal to Raptor that denotes a 'not real' project. Sufficient for our purposes here where we have no project yet.
    /// The ID is consistent across all deployed environments and so acts as a suitable placeholder here.
    /// </summary>
    private const int RAPTOR_CANARY_PROJECT_ID = 987654321;
    
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

    public static LineworkRequest Create(DxfFileRequest fileRequest, string fileDescriptorPathIdentifier)
    {
      // Gets the relative temporary working directory on the Raptor IONode host.
      var rootFolder = $@"D:\ProductionData\Temp\LineworkFileUploads\{fileDescriptorPathIdentifier}\";

      return new LineworkRequest
      {
        ProjectId = RAPTOR_CANARY_PROJECT_ID,
        FileDescriptor = FileDescriptor.CreateFileDescriptor(fileRequest.FilespaceId, @"D:\VLPDProductionData\Temp\12121212\", fileRequest.Filename),
        CoordSystemFileName = fileRequest.CoordinateSystemName?.Trim(),
        LineworkUnits = (TVLPDDistanceUnits)fileRequest.DistanceUnits,
        Filename = fileRequest.Filename,
        FileData = fileRequest.FileData,
        FilespaceId = fileRequest.FilespaceId
      };
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
