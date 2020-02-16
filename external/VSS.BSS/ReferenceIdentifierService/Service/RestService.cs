using System;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.SelfHost;
using log4net;
using Newtonsoft.Json;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.ReferenceIdentifierService.Service
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
      // URL format example: http://localhost:8004/api/CustomerIdentifier/Create
      _config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}");

      _config.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.Objects;

      _server = new HttpSelfHostServer(_config);
    }

    public void Configure(IDependencyResolver dependencyResolver)
    {
      _config.DependencyResolver = dependencyResolver;
    }

    public void Start()
    {
      _server.OpenAsync().Wait();
      Log.IfInfo("Started ReferenceIdentifierService REST API Service");
    }

    public void Stop()
    {
      _server.CloseAsync().Wait();
      _server.Dispose();
      Log.IfInfo("Stopped ReferenceIdentifierService REST API Service");
    }

    public void Error()
    {
      Log.IfInfo("ReferenceIdentifierService REST API Service has thrown an error");
    }
  }
}
