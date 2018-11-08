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
    private readonly IConfigurationStore configStore;
    private string customerUid;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="configStore">IConfigurationStore service implementation</param>
    public RequestFactory(IConfigurationStore configStore) 
    {
      this.configStore = configStore;
    }
    
    /// <inheritdoc />
    public T Create<T>(Action<RequestFactory> action) where T : DataRequestBase, new()
    {
      action(this);

      var obj = new T();
      obj.Initialize(configStore, customerUid);

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