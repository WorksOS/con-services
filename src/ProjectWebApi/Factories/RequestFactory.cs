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
    private IDictionary<string, string> headers;
    private string customerUid;
    private string userId;
    private string userEmailAddress;
    private ProjectRepository projectRepo;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">ILoggerFactory service implementation</param>
    /// <param name="configStore">IConfigurationStore service implementation</param>
    /// <param name="projectRepo"></param>
    public RequestFactory(ILoggerFactory logger, IConfigurationStore configStore,
      IRepository<IProjectEvent> projectRepo)
    {
      log = logger.CreateLogger<ProjectSettingsRequestHelper>();
      this.configStore = configStore;
      this.projectRepo = projectRepo as ProjectRepository;
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
      obj.Initialize(log, configStore, projectRepo);

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

    /// <summary>
    /// Sets the customerUid from the authentication header.
    /// </summary>
    /// <param name="customerUid"></param>
    public RequestFactory CustomerUid(string customerUid)
    {
      this.customerUid = customerUid;
      return this;
    }

    /// <summary>
    /// Sets the userId from the authentication header.
    /// </summary>
    /// <param name="userId"></param>
    public RequestFactory UserId(string userId)
    {
      this.userId = userId;
      return this;
    }

    /// <summary>
    /// Sets the users email address from the authentication header.
    /// </summary>
    /// <param name="userEmailAddress"></param>
    public RequestFactory UserEmailAddress(string userEmailAddress)
    {
      this.userEmailAddress = userEmailAddress;
      return this;
    }

  }
}
