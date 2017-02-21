using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies.Models;

namespace VSS.Raptor.Service.Common.Proxies
{
  /// <summary>
  /// Proxy to get project data from master data service.
  /// </summary>
  public class ProjectProxy : BaseProxy<ProjectData>, IProjectProxy
  {
    private static TimeSpan projectCacheLife = new TimeSpan(0, 15, 0);//TODO: how long to cache ?

    public ProjectProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {    
    }
    /// <summary>
    /// Gets the legacy project ID for a given UID.
    /// </summary>
    /// <param name="projectUid">The project UID</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUId and customerUId)</param>
    /// <returns></returns>
    public long GetProjectId(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      ProjectData cacheData = GetItem(projectUid, projectCacheLife, "PROJECT_URL", customHeaders);
      return cacheData == null ? 0L : cacheData.LegacyProjectId;
    }
  }
}
