using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VSS.ConfigurationStore;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Project.WebAPI.Factories
{
  /// <summary>
  /// 
  /// </summary>
  public class RequestFactory : IRequestFactory
  {
    private readonly ILogger log;
    private readonly IConfigurationStore configStore;
    //private IDictionary<string, string> headers;
    private string customerUid;
    //private string userId;
    //private string userEmailAddress;
    //private ProjectRepository projectRepo;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">ILoggerFactory service implementation</param>
    /// <param name="configStore">IConfigurationStore service implementation</param>
    /// <param name="customerUid"></param>
    public RequestFactory(ILoggerFactory logger, IConfigurationStore configStore, string customerUid)
    {
      log = logger.CreateLogger<ProjectSettingsRequestHelper>();
      this.configStore = configStore;
      this.customerUid = customerUid;
      //this.projectRepo = projectRepo as ProjectRepository;
    }

    /// <summary>
    /// Create instance of T.
    /// </summary>
    /// <typeparam name="T">Derived implementation of DataRequestBase</typeparam>
    /// <returns>Returns instance of T with required attributes set.</returns>
    public T Create<T>(Action<RequestFactory> action) where T : DataRequestBase, new()
    {
      action(this);

      var obj = new T();
      obj.Initialize(log, configStore, customerUid);

      return obj;
    }

    /// <summary>
    /// Sets the customerUid from the authentication header.
    /// </summary>
    /// <param name="customerUid"></param>
    public RequestFactory CustomerUid(string customerUid)
    {
      this.customerUid = customerUid;
      return this;
    }
    
  }
}
