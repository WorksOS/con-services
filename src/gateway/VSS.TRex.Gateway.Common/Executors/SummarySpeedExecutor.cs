using System;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Analytics.SpeedStatistics;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Requests;
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      const string ERROR_MESSAGE = "Failed to get requested machine speed summary data";

      SpeedSummaryRequest request = item as SpeedSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException(typeof(SpeedSummaryRequest));

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var machineSpeedTargetRange = new MachineSpeedExtendedRecord();
      if (request.MachineSpeedTarget != null)
        machineSpeedTargetRange.SetMinMax(request.MachineSpeedTarget.MinTargetMachineSpeed, request.MachineSpeedTarget.MaxTargetMachineSpeed);

      SpeedStatisticsOperation operation = new SpeedStatisticsOperation();
      SpeedSummaryResult speedSummaryResult = operation.Execute(
        new SpeedStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          TargetMachineSpeed = machineSpeedTargetRange
        }
      );

      if (speedSummaryResult != null)
      {
        if (speedSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(speedSummaryResult);

        throw CreateServiceException(ERROR_MESSAGE, speedSummaryResult.ResultStatus);
      }

      throw CreateServiceException(ERROR_MESSAGE);
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
