using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using VSS.Raptor.Service.Common.JsonConverters;

namespace VSS.Raptor.Service.WebApi
{
  public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
  {
    private readonly ILogger<MvcOptions> _logger;
    private readonly ObjectPoolProvider _objectPoolProvider;
    public ConfigureMvcOptions(ILogger<MvcOptions> logger, ObjectPoolProvider objectPoolProvider)
    {
      _logger = logger;
      _objectPoolProvider = objectPoolProvider;
    }

    public void Configure(MvcOptions options)
    {
      options.UseProjectIDJsonInputFormatter(_logger, _objectPoolProvider);
    }
  }
}
