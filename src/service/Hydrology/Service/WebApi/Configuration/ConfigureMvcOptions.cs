using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace VSS.Hydrology.WebApi.Configuration
{
  /// <summary>
  /// 
  /// </summary>
  public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
  {
    private readonly ILogger<MvcOptions> _logger;
    private readonly ObjectPoolProvider _objectPoolProvider;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="objectPoolProvider"></param>
    public ConfigureMvcOptions(ILogger<MvcOptions> logger, ObjectPoolProvider objectPoolProvider)
    {
      _logger = logger;
      _objectPoolProvider = objectPoolProvider;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Configure(MvcOptions options)
    { }
  }
}
