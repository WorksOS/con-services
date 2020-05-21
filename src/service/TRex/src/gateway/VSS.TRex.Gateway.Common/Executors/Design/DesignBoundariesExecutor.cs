using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Gateway.Common.Requests;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get design boundaries from TRex's site model/project.
  /// </summary>
  /// 
  public class DesignBoundariesExecutor : BaseExecutor
  {
    public DesignBoundariesExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DesignBoundariesExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as DesignBoundariesRequest;

      if (request == null)
        ThrowRequestTypeCastException<DesignBoundariesRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var csib = siteModel.CSIB();

      if (csib == string.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          $"The project does not have Coordinate System definition data. Project UID: {siteModel.ID}"));

      var referenceDesign = new DesignOffset(request.DesignUid, 0.0);
      var designBoundaryRequest = new DesignBoundaryRequest();
      var designBoundaryResponse = await designBoundaryRequest.ExecuteAsync(new DesignBoundaryArgument
      {
        ProjectID = siteModel.ID,
        ReferenceDesign = referenceDesign
      });

      if (designBoundaryResponse != null && 
          designBoundaryResponse.RequestResult == DesignProfilerRequestResult.OK && 
          designBoundaryResponse.Boundary != null)
        return await DesignBoundaryHelper.ConvertBoundary(designBoundaryResponse.Boundary, request.Tolerance, siteModel.CellSize, csib, request.FileName);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Design Boundary data"));
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
