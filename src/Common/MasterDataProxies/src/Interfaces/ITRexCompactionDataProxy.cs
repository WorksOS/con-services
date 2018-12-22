using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.MasterData.Proxies.Interfaces
{
  /// <summary>
  /// Proxy interface to access the TRex Gateway WebAPIs.
  /// </summary>
  public interface ITRexCompactionDataProxy
  {
    /// <summary>
    /// Sends a request to get CMV % Change statistics from the TRex database.
    /// </summary>
    /// <param name="cmvChangeDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<CMVChangeSummaryResult> SendCMVChangeDetailsRequest(CMVChangeDetailsRequest cmvChangeDetailsRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get CMV Details statistics from the TRex database.
    /// </summary>
    /// <param name="cmvDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<CMVDetailedResult> SendCMVDetailsRequest(CMVDetailsRequest cmvDetailsRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get CMV Summary statistics from the TRex database.
    /// </summary>
    /// <param name="cmvSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<CMVSummaryResult> SendCMVSummaryRequest(CMVSummaryRequest cmvSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Pass Count Details statistics from the TRex database.
    /// </summary>
    /// <param name="pcDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<PassCountDetailedResult> SendPassCountDetailsRequest(PassCountDetailsRequest pcDetailsRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Pass Count Summary statistics from the TRex database.
    /// </summary>
    /// <param name="pcSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<PassCountSummaryResult> SendPassCountSummaryRequest(PassCountSummaryRequest pcSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Cut/Fill Details statistics from the TRex database.
    /// </summary>
    /// <param name="cfDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<CompactionCutFillDetailedResult> SendCutFillDetailsRequest(CutFillDetailsRequest cfDetailsRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get MDP Summary statistics from the TRex database.
    /// </summary>
    /// <param name="mdpSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<MDPSummaryResult> SendMDPSummaryRequest(MDPSummaryRequest mdpSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Material Temperature Summary statistics from the TRex database.
    /// </summary>
    /// <param name="temperatureSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<TemperatureSummaryResult> SendTemperatureSummaryRequest(TemperatureSummaryRequest temperatureSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Material Temperature Details statistics from the TRex database.
    /// </summary>
    /// <param name="temperatureDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<TemperatureDetailResult> SendTemperatureDetailsRequest(TemperatureDetailRequest temperatureDetailsRequest,
    IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Machine Speed Summary statistics from the TRex database.
    /// </summary>
    /// <param name="speedSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<SpeedSummaryResult> SendSpeedSummaryRequest(SpeedSummaryRequest speedSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get production data tile from the TRex database.
    /// </summary>
    /// <param name="tileRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<Stream> SendProductionDataTileRequest(TileRequest tileRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Summary Volumes statistics from the TRex database.
    /// </summary>
    /// <param name="summaryVolumesRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<SummaryVolumesResult> SendSummaryVolumesRequest(SummaryVolumesDataRequest summaryVolumesRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get project extents for a site model from the TRex database.
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<BoundingBox3DGrid> SendProjectExtentsRequest(string siteModelID,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get a TIN surface data from the TRex database.
    /// </summary>
    /// <param name="compactionExportRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<CompactionExportResult> SendSurfaceExportRequest(CompactionExportRequest compactionExportRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get production data patches from the TRex database.
    /// </summary>
    /// <param name="patchDataRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<Stream> SendProductionDataPatchRequest(PatchDataRequest patchDataRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get station and offset report data from TRex.
    /// </summary>
    /// <param name="stationOffsetRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<Stream> SendStationOffsetReportRequest(CompactionReportStationOffsetRequest stationOffsetRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get grid report data from TRex.
    /// </summary>
    /// <param name="gridRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<Stream> SendGridReportRequest(CompactionReportGridRequest gridRequest,
      IDictionary<string, string> customHeaders = null);
  }
}
