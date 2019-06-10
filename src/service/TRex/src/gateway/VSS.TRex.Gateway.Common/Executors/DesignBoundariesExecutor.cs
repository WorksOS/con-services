using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling;

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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as TRexDesignBoundariesRequest;

      if (request == null)
        ThrowRequestTypeCastException<TRexDesignBoundariesRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      // ...

      return new DesignBoundaryResult();
    }
  }
}
