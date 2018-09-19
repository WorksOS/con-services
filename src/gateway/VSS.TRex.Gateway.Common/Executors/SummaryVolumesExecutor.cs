using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Requests;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get Summary Volumes statistics.
  /// </summary>
  public class SummaryVolumesExecutor : BaseExecutor
  {
    public SummaryVolumesExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryVolumesExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      SummaryVolumesRequest request = item as SummaryVolumesRequest;

      if (request == null)
        ThrowRequestTypeCastException(typeof(SummaryVolumesRequest));

      var siteModel = GetSiteModel(request.ProjectUid);


      SummaryVolumesResult summaryVolumesResult = null;



      if (summaryVolumesResult != null)
        return summaryVolumesResult;

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Summary Volumes data"));
    }
  }
}
