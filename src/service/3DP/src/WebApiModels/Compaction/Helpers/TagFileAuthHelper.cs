﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// To support getting identifying project
  /// </summary>
  public class TagFileAuthHelper : ITagFileAuthHelper
  {
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IConfigurationStore _configStore;
    private readonly ITagFileAuthProjectProxy _tagFileAuthProjectV2Proxy;

    public TagFileAuthHelper(ILoggerFactory loggerFactory, IConfigurationStore configStore,
      ITagFileAuthProjectProxy tagFileAuthProjectV2Proxy
      )
    {
      _loggerFactory = loggerFactory;
      _configStore = configStore;     
      _tagFileAuthProjectV2Proxy = tagFileAuthProjectV2Proxy;
    }

    /// <summary>
    /// identify VSS projectUid (and potentially VSS AssetUID)
    /// tfa checks in this order: snm940; snm941; EC520
    /// </summary>
    public async Task<GetProjectAndAssetUidsResult> GetProjectUid(string radioSerial, string eCSerial,
        string tccOrgUid, double machineLatitude, double machineLongitude)
    {
      var tfaRequest = new GetProjectAndAssetUidsRequest(null,
        (int)DeviceTypeEnum.SNM940, radioSerial, eCSerial,
        tccOrgUid, machineLatitude, machineLongitude, DateTime.UtcNow);

      GetProjectAndAssetUidsResult result;
      try
      {
        result = await _tagFileAuthProjectV2Proxy.GetProjectAndAssetUidsCTCT(tfaRequest);
      }
      catch (Exception e)
      {
        _logger.LogError(e, $"{nameof(GetProjectUid)} TagFileAuth exception thrown: ");
        throw new ServiceException(HttpStatusCode.InternalServerError,
                 new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, e.Message));
      }
      return result;
    }
  }
}