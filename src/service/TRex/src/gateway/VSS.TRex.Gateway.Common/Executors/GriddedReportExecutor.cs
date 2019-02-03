using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CompactionReportGridTRexRequest;

      if (request == null)
      {
        ThrowRequestTypeCastException<CompactionReportGridTRexRequest>();
        return null; // to keep compiler happy
      }

        var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);

      GriddedReportRequest tRexRequest = new GriddedReportRequest();
      var griddedReportRequestArgument = AutoMapperUtility.Automapper.Map<GriddedReportRequestArgument>(request);
      griddedReportRequestArgument.Filters = new FilterSet(filter);

      GriddedReportRequestResponse response = tRexRequest.Execute(griddedReportRequestArgument);

      var result = new GriddedReportResult()
      {
        ReturnCode = response.ReturnCode,
        ReportType = ReportType.Gridded,
        GriddedData = AutoMapperUtility.Automapper.Map<GriddedReportData>(request)
      };
      result.GriddedData.NumberOfRows = response.GriddedReportDataRowList.Count;
      result.GriddedData.Rows.AddRange(response.GriddedReportDataRowList);
      return new GriddedReportDataResult(result.Write());
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
