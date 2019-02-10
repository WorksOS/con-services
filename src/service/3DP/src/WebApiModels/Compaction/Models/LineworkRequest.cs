using System;
using System.Net;
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

    private const string MOCK_FILE_REQUEST_ID = "123";

    public string Filename { get; }
    public byte[] FileData { get; }
    public string FilespaceId { get; }
    public TVLPDDistanceUnits LineworkUnits { get; }
    public string CoordSystemFileName { get; }
    public FileDescriptor FileDescriptor { get; }

    public int NumberOfBoundariesToProcess => string.IsNullOrEmpty(CoordSystemFileName) ? 1 : __Global.MAX_BOUNDARIES_TO_PROCESS;
    public int NumberOfVerticesPerBoundary = __Global.MAX_VERTICES_PER_BOUNDARY;

    public LineworkRequest(DxfFileRequest fileRequest, string uploadPath)
    {
      ProjectId = RAPTOR_CANARY_PROJECT_ID;
      FileDescriptor = FileDescriptor.CreateFileDescriptor(MOCK_FILE_REQUEST_ID, uploadPath, fileRequest.Filename);
      CoordSystemFileName = fileRequest.CoordinateSystemName?.Trim();
      LineworkUnits = (TVLPDDistanceUnits)fileRequest.DxfUnits;
      Filename = fileRequest.Filename;
      FileData = fileRequest.FileData;
      FilespaceId = MOCK_FILE_REQUEST_ID;
    }

    public new LineworkRequest Validate()
    {
      // Don't need to validate project Id or UID on this request object. By design we don't have them.

      if (FileData == null || FileData.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "File data cannot be null"));
      }

      if ((int)LineworkUnits == DxfFileRequest.NOT_DEFINED)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Invalid request, DxfUnits must be provided"));
      }

      if (!Enum.IsDefined(typeof(TVLPDDistanceUnits), LineworkUnits))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Invalid DxfUnits value '{LineworkUnits}', out of range"));
      }

      return this;
    }
  }
}
