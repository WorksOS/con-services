using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Hydrology.WebApi.Middleware
{
  public class HydrologyAuthentication : TIDAuthentication
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="HydrologyAuthentication"/> class.
    /// </summary>
    public HydrologyAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    {
    }    
  }
}

