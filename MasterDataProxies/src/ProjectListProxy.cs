using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MasterDataProxies.Interfaces;
using MasterDataModels.Models;
using MasterDataModels.ResultHandling;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;

namespace MasterDataProxies
{
  public class ProjectListProxy : BaseProxy, IProjectListProxy
  {
    private static TimeSpan projectListCacheLife = new TimeSpan(0, 15, 0);//TODO: how long to cache ?

    public ProjectListProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    public async Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedList<ProjectDataResult>(customerUid, projectListCacheLife, "PROJECT_API_URL", customHeaders);
      if (result.Code == 0)
      {
        return result.ProjectDescriptors;
      }
      else
      {
        log.LogDebug("Failed to get list of projects: {0}, {1}", result.Code, result.Message);
        return null;
      }
    }
  }
}
