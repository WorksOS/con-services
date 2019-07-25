using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Analytics.SpeedStatistics;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Types;
using SpeedSummaryResult = VSS.TRex.Analytics.SpeedStatistics.SpeedStatisticsResult;
using SummaryResult = VSS.Productivity3D.Models.ResultHandling.SpeedSummaryResult;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get machine speed summary.
  /// </summary>
  public class SummarySpeedExecutor : BaseExecutor
  {
    private const int PRECISION_PERCENTAGE = 1;
    private const int PRECISION_AREA = 5;

    public SummarySpeedExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummarySpeedExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as SpeedSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException<SpeedSummaryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new SpeedStatisticsOperation();
      var speedSummaryResult = await operation.ExecuteAsync(
        new SpeedStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
          LiftParams = AutoMapperUtility.Automapper.Map<LiftParameters>(request.LiftSettings)
        }
      );

      if (speedSummaryResult != null)
      {
        if (speedSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(speedSummaryResult);

        throw CreateServiceException<SummarySpeedExecutor>(speedSummaryResult.ResultStatus);
      }

      throw CreateServiceException<SummarySpeedExecutor>();
    }

    private SummaryResult ConvertResult(SpeedSummaryResult result)
    {
      return new SummaryResult
      (
        Math.Round(result.AboveTargetPercent, PRECISION_PERCENTAGE, MidpointRounding.AwayFromZero),
        Math.Round(result.BelowTargetPercent, PRECISION_PERCENTAGE, MidpointRounding.AwayFromZero),
        Math.Round(result.WithinTargetPercent, PRECISION_PERCENTAGE, MidpointRounding.AwayFromZero),
        Math.Round(result.TotalAreaCoveredSqMeters, PRECISION_AREA)
      );
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
