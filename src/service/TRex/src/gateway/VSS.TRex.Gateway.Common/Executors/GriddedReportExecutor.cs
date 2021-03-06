﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.GridFabric;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class GriddedReportExecutor : BaseExecutor
  {
    public GriddedReportExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GriddedReportExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CompactionReportGridTRexRequest>(item);
      var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);

      var tRexRequest = new GriddedReportRequest();
      var griddedReportRequestArgument = AutoMapperUtility.Automapper.Map<GriddedReportRequestArgument>(request);
      griddedReportRequestArgument.Filters = new FilterSet(filter);

      var response = await tRexRequest.ExecuteAsync(griddedReportRequestArgument);
      var result = new GriddedReportResult()
      {
        ReturnCode = response?.ReturnCode ?? ReportReturnCode.UnknownError,
        ReportType = ReportType.Gridded,
        GriddedData = AutoMapperUtility.Automapper.Map<GriddedReportData>(request)
      };
      result.GriddedData.NumberOfRows = response?.GriddedReportDataRowList.Count ?? 0;
      result.GriddedData.Rows.AddRange(response?.GriddedReportDataRowList ?? new List<GriddedReportDataRow>());
      return new GriddedReportDataResult(result.Write());
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
