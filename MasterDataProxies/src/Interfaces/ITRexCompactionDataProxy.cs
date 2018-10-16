using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

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
    Task<ContractExecutionResult> SendCMVChangeDetailsRequest(CMVChangeDetailsRequest cmvChangeDetailsRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get CMV Details statistics from the TRex database.
    /// </summary>
    /// <param name="cmvDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendCMVDetailsRequest(CMVDetailsRequest cmvDetailsRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get CMV Summary statistics from the TRex database.
    /// </summary>
    /// <param name="cmvSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendCMVSummaryRequest(CMVSummaryRequest cmvSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Pass Count Details statistics from the TRex database.
    /// </summary>
    /// <param name="pcDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendPassCountDetailsRequest(PassCountDetailsRequest pcDetailsRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Pass Count Summary statistics from the TRex database.
    /// </summary>
    /// <param name="pcSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendPassCountSummaryRequest(PassCountSummaryRequest pcSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Cut/Fill Details statistics from the TRex database.
    /// </summary>
    /// <param name="cfDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendCutFillDetailsRequest(CutFillDetailsRequest cfDetailsRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get MDP Summary statistics from the TRex database.
    /// </summary>
    /// <param name="mdpSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendMDPSummaryRequest(MDPSummaryRequest mdpSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Material Temperature Summary statistics from the TRex database.
    /// </summary>
    /// <param name="temperatureSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendTemperatureSummaryRequest(TemperatureSummaryRequest temperatureSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Machine Speed Summary statistics from the TRex database.
    /// </summary>
    /// <param name="speedSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendSpeedSummaryRequest(SpeedSummaryRequest speedSummaryRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get production data tile from the TRex database.
    /// </summary>
    /// <param name="tileRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendProductionDataTileRequest(TileRequest tileRequest,
      IDictionary<string, string> customHeaders = null);

    /// <summary>
    /// Sends a request to get Summary Volumes statistics from the TRex database.
    /// </summary>
    /// <param name="summaryVolumesRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendSummaryVolumesRequest(SummaryVolumesDataRequest summaryVolumesRequest,
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
    /// <param name="projectUid"></param>
    /// <param name="filterUid"></param>
    /// <param name="tolerance"></param>
    /// <param name="fileName"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    Task<ContractExecutionResult> SendSurfaceExportRequest(string projectUid, 
      string filterUid, 
      double? tolerance,
      string fileName,
      IDictionary<string, string> customHeaders = null);
  }
}
