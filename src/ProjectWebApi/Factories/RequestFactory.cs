using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VSS.ConfigurationStore;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Repositories;

namespace VSS.MasterData.Project.WebAPI.Factories
{
  /// <summary>
  /// 
  /// </summary>
  public class RequestFactory : IRequestFactory
  {
    private readonly ILogger log;
    private readonly IConfigurationStore configStore;
    private readonly ServiceExceptionHandler serviceExceptionHandler;
    private IDictionary<string, string> headers;
    private ProjectRepository projectRepo;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">ILoggerFactory service implementation</param>
    /// <param name="configStore">IConfigurationStore service implementation</param>
    public RequestFactory(ILoggerFactory logger, IConfigurationStore configStore, ServiceExceptionHandler serviceExceptionHandler, IDictionary<string, string> headers, ProjectRepository projectRepo)
    {
      log = logger.CreateLogger<ProjectSettingsRequestHelper>();
      this.configStore = configStore;
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.headers = headers;
      this.projectRepo = projectRepo;
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
      obj.Initialize(log, configStore, serviceExceptionHandler, headers, projectRepo);

      return obj;
    }

    /// <summary>
    /// Sets the collection of custom headers used on the service request.
    /// </summary>
    /// <param name="headers"></param>
    public RequestFactory Headers(IDictionary<string, string> headers)
    {
      this.headers = headers;
      return this;
    }

  }
}
