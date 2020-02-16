using Autofac;
using Autofac.Extras.DynamicProxy2;
using log4net;
using Magnum.Extensions;
using MassTransit;
using MassTransit.Log4NetIntegration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.ServiceProcess;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.Instrumentation.Interceptors;
using VSS.Nighthawk.NHDataSvc.Common;
using VSS.Nighthawk.NHDataSvc.DataAccess;
using VSS.Nighthawk.NHDataSvc.Helpers;
using VSS.Nighthawk.NHDataSvc.Interfaces.Helpers;
using VssJsonMessageSerializer = VSS.Nighthawk.MassTransit.JsonMessageSerializer;

namespace VSS.Nighthawk.NHDataSvc
{
  /// <summary>
  /// Windows Service host service. Provides the framework to start/stop the hosted service.
  /// </summary>
  public partial class NHDataSvc : ServiceBase
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private IContainer container;

    public NHDataSvc()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      log.IfInfo("NHDataSvc starting up...");

#if DEBUG
      System.Threading.Thread.Sleep(TimeSpan.FromSeconds(20));
#endif
      //System.Diagnostics.Debugger.Launch();
      //System.Diagnostics.Debugger.Break();

      AppDomain.CurrentDomain.UnhandledException += UnexpectedExceptionHandler;

      try
      {
        var builder = new ContainerBuilder();

        VssJsonMessageSerializer.AddConverter<INHDataObjectConverter>();

        builder.Register<INHDataProcessor>(c =>
        {
          var processor = SpringObjectFactory.CreateObject<NHDataProcessor>();
          processor.AlertSvc = new AlertSvc();
          return processor;
        }).SingleInstance().PropertiesAutowired();

        IConnectionConfig connectionConfig = new RabbitMqConnectionConfig();

        string connectionString = connectionConfig.ConnectionString();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
       .Where(t => t.Implements<IConsumer>())
       .AsSelf().SingleInstance()
       .EnableClassInterceptors()
       .InterceptedBy(typeof(MessageConsumerInterceptor));

        builder.Register(c => new MessageConsumerInterceptor());

        builder.RegisterGeneric(typeof(DocumentStore<>)).As(typeof(IDocumentStore<>)).SingleInstance();

        builder.RegisterType<VehicleMappingLookupCache>().AsSelf().SingleInstance();
        builder.Register(e =>
        {
          VssBsonMediaTypeFormatter bson = new VssBsonMediaTypeFormatter();
          bson.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

          var json = new JsonMediaTypeFormatter();
          json.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
          return new List<MediaTypeFormatter> 
          { 
            json, 
            bson 
          };
        }).As<List<MediaTypeFormatter>>().SingleInstance();

        builder.Register(e =>
        {
          HttpClient client = new HttpClient();
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/bson"));
          client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
          return client;
        }).As<HttpClient>();

        builder.Register(c => ServiceBusFactory.New(sbc =>
        {
          sbc.UseRabbitMq(
            cnfg => cnfg.ConfigureHost(new Uri(connectionString),
                                       configurator =>
                                       {
                                         configurator.SetUsername(
                                           connectionConfig.GetUserName());
                                         configurator.SetPassword(
                                           connectionConfig.GetPassword());
                                         configurator.SetRequestedHeartbeat(connectionConfig.GetHeartbeatSeconds());
                                       }));
          sbc.ReceiveFrom(connectionString);
          sbc.UseControlBus();
          sbc.SetConcurrentConsumerLimit(NHDataSettings.Default.ConsumerLimit);
          sbc.SetDefaultSerializer<VssJsonMessageSerializer>();
          sbc.Subscribe(subs => subs.LoadFrom(container));
          sbc.UseLog4Net();
        })).SingleInstance();
        container = builder.Build();

        ((NHDataProcessor)container.Resolve<INHDataProcessor>()).Start();
        container.Resolve<IServiceBus>();

        log.IfInfo("Finished starting service");
      }
      catch (Exception ex)
      {
        log.Fatal("An essential NHData hosted service failed to start up. The hosting service is terminating.", ex);
        throw;
      }
    }

    private static void UnexpectedExceptionHandler(object obj, UnhandledExceptionEventArgs e)
    {
      log.FatalFormat("Fatal error. NHDataSvc IsTerminating = {0}. Exception: {1}", e.IsTerminating, e.ExceptionObject);
    }

    protected override void OnStop()
    {
      log.IfInfo("NHDataSvc stopping...");

      try
      {
        container.Resolve<IServiceBus>().Dispose();
        ((NHDataProcessor)container.Resolve<INHDataProcessor>()).Stop();
      }
      catch (Exception ex)
      {
        log.IfError("A hosted NHData svc failed to stop successfully.", ex);
      }

      log.Info("NHDataSvc Stopped");
    }
  }
}
