using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.Reports.Servers.Client;

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
      var request = item as CompactionReportGridRequest;

      if (request == null)
      { 
        ThrowRequestTypeCastException<CompactionReportGridRequest>();
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "shouldn't get here"); // to keep compiler happy
      }

      var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);
      
      var server = new GriddedReportRequestServer();

      GriddedReportResult result = server.Execute(new GriddedReportRequestArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        ReferenceDesignID = request.DesignFile.Uid ?? Guid.Empty, // only present if ReportCutFill required
        ReportElevation = request.ReportElevation,
        ReportCutFill = request.ReportCutFill,
        ReportCMV = request.ReportCMV,
        ReportMDP = request.ReportMDP,
        ReportPassCount = request.ReportPassCount,
        ReportTemperature = request.ReportTemperature,
        GridInterval = request.GridInterval,
        GridReportOption = request.GridReportOption,
        StartNorthing = request.StartNorthing,
        StartEasting = request.StartEasting,
        EndNorthing = request.EndNorthing,
        EndEasting = request.EndEasting,
        Azimuth = request.Azimuth
      });

      return new ReportGridDataResult(result.Write());
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
