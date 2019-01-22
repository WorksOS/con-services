using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Reports.StationOffset;
using VSS.TRex.Reports.StationOffset.GridFabric;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class StationOffsetReportExecutor : BaseExecutor
  {
    public StationOffsetReportExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public StationOffsetReportExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CompactionReportStationOffsetTRexRequest;

      if (request == null)
      {
        ThrowRequestTypeCastException<CompactionReportStationOffsetTRexRequest>();
        return null; // to keep compiler happy
      }

      var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);

      StationOffsetReportRequest tRexRequest = new StationOffsetReportRequest();

      StationOffsetReportRequestResponse response = tRexRequest.Execute(new StationOffsetReportRequestArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        ReferenceDesignUID = request.CutFillDesignUid ?? Guid.Empty, // only present if ReportCutFill required
        ReportElevation = request.ReportElevation,
        ReportCutFill = request.ReportCutFill,
        ReportCmv = request.ReportCmv,
        ReportMdp = request.ReportMdp,
        ReportPassCount = request.ReportPassCount,
        ReportTemperature = request.ReportTemperature,
        AlignmentDesignUid = request.AlignmentDesignUid,
        CrossSectionInterval = request.CrossSectionInterval,
        StartStation = request.StartStation,
        EndStation = request.EndStation,
        Offsets = request.Offsets
      });

      var result = new StationOffsetReportResult()
      {
        ReturnCode = response.ReturnCode,
        ReportType = ReportType.Gridded,
        GriddedData = new StationOffsetReportData()
        {
          ElevationReport = request.ReportElevation,
          CutFillReport = request.ReportCutFill,
          CmvReport = request.ReportCmv,
          MdpReport = request.ReportMdp,
          PassCountReport = request.ReportPassCount,
          TemperatureReport = request.ReportTemperature,
          NumberOfRows = response.StationOffsetReportDataRowList.Count
        }
      };
      result.GriddedData.Rows.AddRange(response.StationOffsetReportDataRowList);
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
