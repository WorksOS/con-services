using System;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;

namespace VSS.AWS.TransferProxy
{
  public class TransferProxyFactory : ITransferProxyFactory
  {
    private bool _useLocalTransferProxy { get; }
    private IConfigurationStore _config { get; }
    private ILoggerFactory _loggerFactory { get; }

    public TransferProxyFactory(IConfigurationStore config, ILoggerFactory loggerFactory)
    {
      _config = config;
      _loggerFactory = loggerFactory;
      _useLocalTransferProxy = config.GetValueBool("USE_LOCAL_S3_TRANSFER_PROXY_STORE", false);
    }

    /// <summary>
    /// Creates a new S3 transfer proxy given the S3 storage key
    /// </summary>
    /// <param name="storageKey"></param>
    /// <returns></returns>
    public ITransferProxy NewProxy(string storageKey)
    {
      return _useLocalTransferProxy
        ? new LocalTransferProxy(_config, _loggerFactory.CreateLogger<LocalTransferProxy>(), storageKey)
        : new TransferProxy(_config, _loggerFactory.CreateLogger<TransferProxy>(), storageKey) as ITransferProxy;
    }

    public ITransferProxy NewProxy(TransferProxyType type)
    {
      return type switch
      {
        TransferProxyType.DesignImport => NewProxy("AWS_DESIGNIMPORT_BUCKET_NAME"),
        TransferProxyType.Temporary => NewProxy("AWS_TEMPORARY_BUCKET_NAME"),
        TransferProxyType.TAGFiles => NewProxy("AWS_TAGFILE_BUCKET_NAME"), 
        TransferProxyType.Export => NewProxy("AWS_TEMPORARY_BUCKET_NAME"), // Exports go to temporary bucket currently
        TransferProxyType.TagFileGatewayArchive => NewProxy("AWS_ALL_TAGFILE_BUCKET_NAME"),
        
        _ => throw new ArgumentException($"Unsupported transfer proxy type {type}")
      };
    }
  }
}
