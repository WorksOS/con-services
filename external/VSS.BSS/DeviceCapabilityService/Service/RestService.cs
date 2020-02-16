using System;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.SelfHost;
using log4net;

using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.DeviceCapabilityService.Service
{
  public class RestService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly HttpSelfHostServer _server;
    private readonly HttpSelfHostConfiguration _config;

    public RestService(Uri address)
    {
      Log.IfInfoFormat("Creating server at {0}", address.ToString());

      _config = new HttpSelfHostConfiguration(address);
      // URL format example: http://[environment]/api/siteadministration/isitedispatchedevent/1
      _config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}");
      
      _server = new HttpSelfHostServer(_config);
    }

    public void Configure(IDependencyResolver dependencyResolver)
    {
      _config.DependencyResolver = dependencyResolver;
    }

    public void Start()
    {
      _server.OpenAsync().Wait();
      Log.IfInfo("Started DeviceCapability REST API Service");
    }

    public void Stop()
    {
      _server.CloseAsync().Wait();
      _server.Dispose();
      Log.IfInfo("Stopped DeviceCapability REST API Service");
    }

    public void Error()
    {
      Log.IfInfo("DeviceCapability REST API Service has thrown an error");
    }
  }
}
