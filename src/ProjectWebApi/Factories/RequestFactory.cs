using Microsoft.Extensions.Logging;
using System;
using VSS.ConfigurationStore;
using VSS.MasterData.Project.WebAPI.Common.Helpers;

namespace VSS.MasterData.Project.WebAPI.Factories
{
  /// <summary>
  /// Factory for creating <see cref="DataRequestBase"/> instances.
  /// </summary>
  public class RequestFactory : IRequestFactory
  {
    private readonly ILogger log;
    private readonly IConfigurationStore configStore;
    private string customerUid;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">ILoggerFactory service implementation</param>
    /// <param name="configStore">IConfigurationStore service implementation</param>
    public RequestFactory(ILoggerFactory logger, IConfigurationStore configStore) 
    {
      log = logger.CreateLogger<ProjectSettingsRequestHelper>();
      this.configStore = configStore;
    }
    
    /// <inheritdoc />
    public T Create<T>(Action<RequestFactory> action) where T : DataRequestBase, new()
    {
      action(this);

      var obj = new T();
      obj.Initialize(log, configStore, customerUid);

      return obj;
    }
    
    /// <inheritdoc />
    public RequestFactory CustomerUid(string customerUid)
    {
      this.customerUid = customerUid;
      return this;
    }
  }
}