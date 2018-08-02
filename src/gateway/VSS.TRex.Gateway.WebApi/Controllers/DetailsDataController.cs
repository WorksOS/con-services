using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting production data for details requests.
  /// </summary>
  public class DetailsDataController : BaseController
  {
    private IImmutableClientServer reportClientServer;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    /// <param name="reportClientServer"></param>
    protected DetailsDataController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore, IImmutableClientServer reportClientServer) 
      : base(loggerFactory, loggerFactory.CreateLogger<DetailsDataController>(), serviceExceptionHandler, configStore)
    {
      this.reportClientServer = reportClientServer;
    }
  }
}
