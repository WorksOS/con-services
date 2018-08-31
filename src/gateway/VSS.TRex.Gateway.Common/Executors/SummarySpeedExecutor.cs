using System;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Analytics.SpeedStatistics;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Types;
using SpeedSummaryResult = VSS.TRex.Analytics.SpeedStatistics.SpeedResult;
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      SpeedSummaryRequest request = item as SpeedSummaryRequest;

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.filter, siteModel);

      var machineSpeedTargetRange = new MachineSpeedExtendedRecord();
      if (request.machineSpeedTarget != null)
        machineSpeedTargetRange.SetMinMax(request.machineSpeedTarget.MinTargetMachineSpeed, request.machineSpeedTarget.MaxTargetMachineSpeed);

      SpeedOperation operation = new SpeedOperation();
      SpeedSummaryResult speedSummaryResult = operation.Execute(
        new SpeedStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          TargetMachineSpeed = machineSpeedTargetRange
        }
      );

      if (speedSummaryResult != null)
        return ConvertResult(speedSummaryResult);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested machine speed summary data"));
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
  }
}
