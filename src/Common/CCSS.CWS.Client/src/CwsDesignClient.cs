using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// These use the cws cws-designmanager controller
  ///   See comments in CwsAccountClient re TRN/Guid conversions
  /// </summary>
  public class CwsDesignClient : CwsDesignManagerClient, ICwsDesignClient
  {
    public CwsDesignClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// POST http://api-stg.trimble.com/t/trimble.com/cws-designmanager-stg/1.0/projects/{projectId}/file
    ///   user token
    ///   used by ProjectSvc v6 and v5TBC
    /// </summary>
    public async Task<CreateFileResponseModel> CreateFile(Guid projectUid, CreateFileRequestModel createFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateFile)}: createFileRequest {JsonConvert.SerializeObject(createFileRequest)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      var createFileResponse = await PostData<CreateFileRequestModel, CreateFileResponseModel>($"/projects/{projectTrn}/file", createFileRequest, null, customHeaders);

      log.LogDebug($"{nameof(CreateFile)}: createFileResponse {JsonConvert.SerializeObject(createFileResponse)}");
      return createFileResponse;
    }   
  }
}
