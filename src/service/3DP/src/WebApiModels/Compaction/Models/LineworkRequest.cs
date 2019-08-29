using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Request DTO from 3DP to Raptor/TRex.
  /// </summary>
  public class LineworkRequest : ProjectID
  {
    private enum DistanceUnits
    {
      Meters,
      ImperialFeet,
      USSurveyFeet
    }

    private const string MOCK_FILE_REQUEST_ID = "123";
    public byte[] DxfFileData { get; private set; }
    public byte[] CoordinateSystemFileData { get; }
    public string FilespaceId { get; }
    public int LineworkUnits { get; }
    public FileDescriptor DxfFileDescriptor { get; }
    public FileDescriptor CoordinateSystemFileDescriptor { get; }
    private int MaxBoundariesToProcess { get; }

    public int NumberOfBoundariesToProcess => MaxBoundariesToProcess < 1 ? VelociraptorConstants.MAX_BOUNDARIES_TO_PROCESS : MaxBoundariesToProcess;
    public int NumberOfVerticesPerBoundary = VelociraptorConstants.MAX_VERTICES_PER_BOUNDARY;

    public LineworkRequest(DxfFileRequest fileRequest, string uploadPath)
    {
      var tmpFilename = Guid.NewGuid().ToString();

      ProjectId = VelociraptorConstants.NO_PROJECT_ID;
      DxfFileDescriptor = FileDescriptor.CreateFileDescriptor(MOCK_FILE_REQUEST_ID, uploadPath, tmpFilename + ".dxf");
      CoordinateSystemFileDescriptor = FileDescriptor.CreateFileDescriptor(MOCK_FILE_REQUEST_ID, uploadPath, tmpFilename + ".dc");
      LineworkUnits = fileRequest.DxfUnits;
      DxfFileData = fileRequest.GetFileAsByteArray(fileRequest.DxfFile);
      CoordinateSystemFileData = fileRequest.GetFileAsByteArray(fileRequest.CoordinateSystemFile);
      FilespaceId = MOCK_FILE_REQUEST_ID;
      MaxBoundariesToProcess = fileRequest.MaxBoundariesToProcess;
    }

    public void ClearFileData() => DxfFileData = null;

    public new LineworkRequest Validate()
    {
      // Don't need to validate project Id or UID on this request object. By design we don't have them.

      if (DxfFileData == null || DxfFileData.Length == 0)
      {
        ThrowValidationError("DXF file cannot be null");
      }

      if (CoordinateSystemFileData == null || CoordinateSystemFileData.Length == 0)
      {
        ThrowValidationError("Coordinate system file cannot be null");
      }
       
      if (LineworkUnits == DxfFileRequest.NOT_DEFINED)
      {
        ThrowValidationError("Invalid request, DxfUnits must be provided");
      }

      if (!Enum.IsDefined(typeof(DistanceUnits), LineworkUnits))
      {
        ThrowValidationError($"Invalid DxfUnits value '{LineworkUnits}', out of range");
      }

      return this;
    }

    private static void ThrowValidationError(string message)
    {
      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, message));
    }
  }
}
