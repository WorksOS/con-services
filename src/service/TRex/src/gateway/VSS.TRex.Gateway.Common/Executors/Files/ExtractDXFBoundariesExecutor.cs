using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CoreX.Interfaces;
using CoreX.Types;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Files;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Files.DXF;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors.Files
{
  /// <summary>
  /// Processes the request to get boundaries from DXF
  /// </summary>
  public class ExtractDXFBoundariesExecutor : BaseExecutor
  {
    public ExtractDXFBoundariesExecutor(IConfigurationStore configStore, ILoggerFactory logger,
  IServiceExceptionHandler exceptionHandler)
  : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ExtractDXFBoundariesExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as DXFBoundariesRequest;

      if (request == null) 
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Request is null"));

      var result = DXFFileUtilities.RequestBoundariesFromLineWork
        (request.DXFFileData, request.FileUnits, request.MaxBoundaries, request.ConvertLineStringCoordsToPolygon, out var boundaries);

      if (result != DXFUtilitiesResult.Ok)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Error processing file: {result}"));

      if (boundaries == null)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Internal request successful, but returned boundaries are null"));

      return ConvertResult(boundaries, request.CSIBFileData);
    }

    protected DXFBoundaryResult ConvertResult(PolyLineBoundaries boundaries, string coordinateSystemFileData)
    {
      DXFBoundaryResult ConstructResponse()
      {
        return new DXFBoundaryResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Success",
          boundaries.Boundaries.Select(x =>
            new DXFBoundaryResultItem(x.Boundary.Points.Select(pt =>
              new WGSPoint(pt.Y, pt.X)).ToList(), x.Type, x.Name)).ToList());
      }

      if (string.IsNullOrEmpty(coordinateSystemFileData))
      {
        // No coordinate system provided, jsut return the grid coordinates read from the file
        return ConstructResponse();
      }

      var csib = DIContext.Obtain<IConvertCoordinates>().GetCSIBFromDCFileContent(coordinateSystemFileData);

      // Convert grid coordinates into WGS: assemble and convert. Note: 2D conversion only, elevation is set to 0
      //Note YXZ is correct here as it's treated as NEE
      var coordinates = boundaries.Boundaries.SelectMany(x => x.Boundary.Points).Select(pt => new XYZ(x: pt.X, y: pt.Y, z: 0.0)).ToArray();

      // Perform conversion
      var LLHCoords = DIContext.Obtain<IConvertCoordinates>().NEEToLLH(csib, coordinates.ToCoreX_XYZ(), ReturnAs.Degrees);

      // Recopy converted coordinates into boundaries
      var indexer = 0;
      for (var i = 0; i < boundaries.Boundaries.Count; i++)
      {
        var boundary = boundaries.Boundaries[i].Boundary;
        for (var j = 0; j < boundary.NumVertices; j++)
        {
          boundary.Points[j] = new FencePoint(LLHCoords[indexer].X, LLHCoords[indexer].Y, 0.0);
          indexer++;
        }
      }

      return ConstructResponse();
    }

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
