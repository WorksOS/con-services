using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
  public class MockCwsProjectClient : CwsProfileManagerClient, ICwsProjectClient
  {
    public MockCwsProjectClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    public Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IDictionary<string, string> customHeaders = null)
    {
      var createProjectResponseModel = new CreateProjectResponseModel
      {
        Id = Guid.NewGuid().ToString()
      };
      return Task.FromResult(createProjectResponseModel);
    }

    public Task UpdateProjectDetails(Guid projectUid, UpdateProjectDetailsRequestModel updateProjectDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      return Task.CompletedTask;
    }

    public Task UpdateProjectBoundary(Guid projectUid, ProjectBoundary projectBoundary, IDictionary<string, string> customHeaders = null)
    {
      return Task.CompletedTask;
    }
  }
}
