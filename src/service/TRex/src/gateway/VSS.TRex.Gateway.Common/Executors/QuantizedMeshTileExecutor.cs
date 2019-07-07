using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.QuantizedMesh.GridFabric.Requests;
using VSS.TRex.QuantizedMesh.GridFabric.Arguments;
using VSS.TRex.QuantizedMesh.GridFabric.Responses;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class QuantizedMeshTileExecutor : BaseExecutor
  {

    public QuantizedMeshTileExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public QuantizedMeshTileExecutor()
    {
    }


    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as QMTileRequest;

      if (request == null)
        ThrowRequestTypeCastException<QMTileRequest>();

      BoundingWorldExtent3D extents = null;
      bool hasGridCoords = false;
      if (request.BoundBoxLatLon != null)
      {
        extents = AutoMapperUtility.Automapper.Map<BoundingBox2DLatLon, BoundingWorldExtent3D>(request.BoundBoxLatLon);
      }
      else// if (request.BoundBoxGrid != null)
      {
        // throw exception
      //  hasGridCoords = true;
      //  extents = AutoMapperUtility.Automapper.Map<BoundingBox2DGrid, BoundingWorldExtent3D>(request.BoundBoxGrid);
      }

      var siteModel = GetSiteModel(request.ProjectUid);

      var tileRequest = new QuantizedMeshRequest();
      var response = tileRequest.Execute(new QuantizedMeshRequestArgument
        (siteModel.ID,
        extents,
        new FilterSet(ConvertFilter(request.Filter1, siteModel), null))
        ) as DummyQMResponse;

   //   return new QMTileResult(response.TileQMData);


      // return new QMTileRequest(reponse);

      /* todo make quantized mesh
      var tileRequest = new TileRenderRequest();
      var response = tileRequest.Execute(
        new TileRenderRequestArgument
        (siteModel.ID,
          request.Mode,
          ConvertColorPalettes(request, siteModel),
          extents,
          hasGridCoords,
          request.Width, // PixelsX
          request.Height, // PixelsY
          new FilterSet(ConvertFilter(request.Filter1, siteModel), ConvertFilter(request.Filter2, siteModel)),
          new DesignOffset(request.DesignDescriptor?.FileUid ?? Guid.Empty, request.DesignDescriptor.Offset)
        )) as TileRenderResponse_Core2;
        */

      // return dummy data for now
      // need repository for level 0 static files
      var dummyResponse = new byte[] { 0x41, 0x42, 0x41, 0x42, 0x41, 0x42, 0x41 };
      return new QMTileResult(dummyResponse);
    }


  }
}
