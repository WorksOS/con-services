using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client.MockClients
{
  /// <summary>
  /// Mocks to use until we can get the real endpoints
  /// </summary>
  public class MockCwsDesignClient : CwsDesignManagerClient, ICwsDesignClient
  {
    public MockCwsDesignClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    public Task<CreateFileResponseModel> CreateFile(Guid projectUid, CreateFileRequestModel createFileRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateFile)} Mock: createFileRequest {JsonConvert.SerializeObject(createFileRequest)}");

      var createFileResponse = new CreateFileResponseModel
      {
        FileSpaceId = Guid.NewGuid().ToString(),
        UploadUrl = "uploadeurl: FileSpaceId"
      };

      log.LogDebug($"{nameof(CreateFile)} Mock: createFileResponse {JsonConvert.SerializeObject(createFileResponse)}");
      return Task.FromResult(createFileResponse);
    }

    public Task<CreateFileResponseModel> CreateAndUploadFile(Guid projectUid, CreateFileRequestModel createFileRequest, Stream fileContents, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateAndUploadFile)} Mock: createFileRequest {JsonConvert.SerializeObject(createFileRequest)}");

      var createFileResponse = new CreateFileResponseModel
      {
        FileSpaceId = Guid.NewGuid().ToString(),
        UploadUrl = "uploadeurl: FileSpaceId"
      };

      log.LogDebug($"{nameof(CreateAndUploadFile)} Mock: createFileResponse {JsonConvert.SerializeObject(createFileResponse)}");
      return Task.FromResult(createFileResponse);
    }
  }
}
