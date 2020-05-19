using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Request DTO from 3DP to Raptor/TRex.
  /// </summary>
  public class LineworkRequest : ProjectID
  {
    /// <summary>
    /// Base64 encoded content of DXF file
    /// </summary>
    public string DxfFileData { get; private set; }

    /// <summary>
    /// Base64 encoded content of DXF file
    /// </summary>
    public string CoordinateSystemFileData { get; }

    public int LineworkUnits { get; }

    public int MaxBoundariesToProcess { get; }

    public bool ConvertLineStringCoordsToPolygon { get; }

    public int NumberOfBoundariesToProcess => MaxBoundariesToProcess < 1 ? VelociraptorConstants.MAX_BOUNDARIES_TO_PROCESS : MaxBoundariesToProcess;
    public int NumberOfVerticesPerBoundary = VelociraptorConstants.MAX_VERTICES_PER_BOUNDARY;

    public LineworkRequest(DxfFileRequest fileRequest)
    {
      ProjectId = VelociraptorConstants.NO_PROJECT_ID;
      LineworkUnits = fileRequest.DxfUnits;
      DxfFileData = fileRequest.GetFileAsBase64EncodedString(fileRequest.DxfFile);
      CoordinateSystemFileData = fileRequest.GetFileAsBase64EncodedString(fileRequest.CoordinateSystemFile);
      MaxBoundariesToProcess = fileRequest.MaxBoundariesToProcess;
      ConvertLineStringCoordsToPolygon = fileRequest.ConvertLineStringCoordsToPolygon;
    }

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

      if (!Enum.IsDefined(typeof(DxfUnitsType), LineworkUnits))
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
