using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Types;
using TargetPassCountRange = VSS.Productivity3D.Models.Models.TargetPassCountRange;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get Pass Count details.
  /// </summary>
  public class DetailedPassCountExecutor : BaseExecutor
  {
    public DetailedPassCountExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedPassCountExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as PassCountDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException<PassCountDetailsRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new PassCountStatisticsOperation();
      var passCountDetailsResult = await operation.ExecuteAsync(new PassCountStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        PassCountDetailValues = UpdatePassCounts(request.PassCounts),
        Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
        LiftParams = ConvertLift(request.LiftSettings, request.Filter?.LayerType)
      });

      if (passCountDetailsResult != null)
      {
        if (passCountDetailsResult.ResultStatus == RequestErrorStatus.OK)
          return new PassCountDetailedResult(
            new TargetPassCountRange(
              passCountDetailsResult.ConstantTargetPassCountRange.Min,
              passCountDetailsResult.ConstantTargetPassCountRange.Max),
            passCountDetailsResult.IsTargetPassCountConstant,
            passCountDetailsResult.Percents,
            passCountDetailsResult.TotalAreaCoveredSqMeters
          );

        throw CreateServiceException<DetailedPassCountExecutor>(passCountDetailsResult.ResultStatus);
      }

      throw CreateServiceException<DetailedPassCountExecutor>();
    }

    private int[] UpdatePassCounts(int[] passCounts)
    {
      var passCountList = new List<int>();

      passCountList.AddRange(passCounts);
      passCountList.Insert(0, 0);
      passCountList.Add(passCountList.Last() + 1);

      return passCountList.ToArray();

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
