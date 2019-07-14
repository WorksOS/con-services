using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Geometry;

namespace VSS.TRex.Gateway.Common.Executors.Design
{
  /// <summary>
  /// Processes the request to get design filter boundary from TRex's site model/project.
  /// </summary>
  /// 
  public class DesignFilterBoundaryExecutor : BaseExecutor
  {
    public DesignFilterBoundaryExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DesignFilterBoundaryExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as DesignFilterBoundaryRequest;

      if (request == null)
        ThrowRequestTypeCastException<DesignFilterBoundaryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var designFilterBoundaryRequest = new AlignmentDesignFilterBoundaryRequest();
      var referenceDesign = new DesignOffset(request.DesignUid, 0.0);

      var designFilterBoundaryResponse = designFilterBoundaryRequest.Execute(new AlignmentDesignFilterBoundaryArgument()
      {
        ProjectID = siteModel.ID,
        ReferenceDesign = referenceDesign,
        StartStation = request.StartStation,
        EndStation = request.EndStation,
        LeftOffset = request.LeftOffset,
        RightOffset = request.RightOffset
      });


      if (designFilterBoundaryResponse != null &&
          designFilterBoundaryResponse.RequestResult != DesignProfilerRequestResult.OK &&
          designFilterBoundaryResponse.Boundary != null)
        return ConvertResult(designFilterBoundaryResponse.Boundary);
      
      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Design Filter Boundary data"));
    }

    private DesignFilterBoundaryResult ConvertResult(Fence boundary)
    {
      return new DesignFilterBoundaryResult(boundary.Points.Select(p => new WGSPoint(p.Y, p.X)).ToList());
    }
  }
}
