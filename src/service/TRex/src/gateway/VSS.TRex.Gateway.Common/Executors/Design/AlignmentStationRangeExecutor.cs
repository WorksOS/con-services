using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;
using VSS.TRex.Gateway.Common.Requests;

namespace VSS.TRex.Gateway.Common.Executors.Design
{
  /// <summary>
  /// Processes the request to get design alignment station range from TRex's site model/project.
  /// </summary>
  /// 
  public class AlignmentStationRangeExecutor : BaseExecutor
  {
    public AlignmentStationRangeExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public AlignmentStationRangeExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as DesignDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<DesignDataRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var alignmentDesignStationRangeRequest = new AlignmentDesignStationRangeRequest();
      var referenceDesign = new DesignOffset(request.DesignUid, 0.0);

      var alignmentDesignStationRangeResponse = alignmentDesignStationRangeRequest.Execute(new DesignSubGridRequestArgumentBase()
      {
        ProjectID = siteModel.ID,
        ReferenceDesign = referenceDesign
      });

      if (alignmentDesignStationRangeResponse != null &&
          alignmentDesignStationRangeResponse.RequestResult != DesignProfilerRequestResult.OK)
        return new AlignmentStationRangeResult(alignmentDesignStationRangeResponse.StartStation, alignmentDesignStationRangeResponse.EndStation);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Alignment Design station range data."));
    }
  }
}
