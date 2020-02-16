using System;
using System.Configuration;
using System.ServiceProcess;
using log4net;
using MassTransit;
using MassTransit.Log4NetIntegration;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Factories;
using VSS.Nighthawk.MassTransit;
using VSS.Nighthawk.NHOPSvc.ConfigStatus;
using VSS.Nighthawk.NHOPSvc.Helpers;
using VSS.Nighthawk.NHOPSvc.Consumer;
using VSS.Nighthawk.NHOPSvc.Interfaces.Helpers;

namespace VSS.Nighthawk.NHOPSvc
{
  /// <summary>
  /// The Windows Service hosting service. Provides a framework to start/stop the hosted services.
  /// 
  /// This Windows service hosts the following Nighthawk services:
  /// <list type="bullet">
  /// <item>Site Determination</item>
  /// <item>Config Status service</item>
  /// <item>Alert Trigger service</item>
  /// <item>BSS Feedback service</item>
  /// <item>Email service</item>
  /// <item>ETL Service</item>
  /// </list>
  /// </summary>
  public partial class NHOPSvc : ServiceBase
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    public  static  object SyncCulture = new object();
    private MTSConfigManager configRetriever;
    private ConfigStatusSvc configStatusSvc;
    private NHOPDataObjectEventConsumer eventConsumer;

    private static IServiceBus _serviceBus = null;

    public NHOPSvc()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      log.IfInfo("NHOpSvc starting up...");      
#if DEBUG
      // Pause to allow time to attach the debugger
      System.Threading.Thread.Sleep(30);
#endif
      
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnexpectedExceptionHandler);
      try
      {
          InitServiceBus();
       
        if (NHOPSvcSettings.Default.ConfigStatusSvcEnabled)
        {
            configStatusSvc = new ConfigStatusSvc();
            configStatusSvc.Start();
            configRetriever = new MTSConfigManager(configStatusSvc);
            configRetriever.Start();
            eventConsumer = new NHOPDataObjectEventConsumer(configStatusSvc);
            _serviceBus.SubscribeInstance(eventConsumer);
            log.IfInfo("Configuration Status Service Started");
        }    
        
      }
      catch (Exception e)
      {
        log.IfFatal("Failed to start all essential NHOP services.", e);
      }
    }

    private static void UnexpectedExceptionHandler(object obj, UnhandledExceptionEventArgs e)
    {
      log.FatalFormat("Fatal error. NHOpSvc IsTerminating = {0}. Exception: {1}", e.IsTerminating, e.ExceptionObject);
    }

    protected override void OnStop()
    {
      if (NHOPSvcSettings.Default.ConfigStatusSvcEnabled)
      {
        configStatusSvc.Stop();
        configRetriever.Stop();
      }
      
      if(null != _serviceBus)
      {
        _serviceBus.Dispose();
        _serviceBus = null;
      }
      log.IfInfo("Configuration Status Service Stopped");
    }

    private void InitServiceBus()
    {
      JsonMessageSerializer.AddConverter<INHOPDataObjectConverter>();
      IConnectionConfig connectionConfig = new RabbitMqConnectionConfig();
      var connectionString = connectionConfig.ConnectionString(csName: "NH_RABBITMQv2",
                                                             keyVirtualHost: "RabbitMqVirtualHost",
                                                             keyQName: "RabbitMqName");
      if (connectionConfig.HasConfigError)
      {
        throw new Exception("Issue in RabbitMQ configuration for DataOut CommandApi Client");
      }

      log.Info("Connection string : " + connectionString);
      
      if (connectionString != string.Empty)
      {
        try
        {
          _serviceBus = ServiceBusFactory.New(sbc =>
          {
            sbc.UseRabbitMq(
              cnfg => cnfg.ConfigureHost(new Uri(connectionString),
                configurator =>
                {
                  configurator.SetUsername(connectionConfig.GetUserName(keyUser: "RabbitMqUser"));
                  configurator.SetPassword(connectionConfig.GetPassword(keyPassword: "RabbitMqPassword"));
                  configurator.SetRequestedHeartbeat(connectionConfig.GetHeartbeatSeconds());
                }));
            sbc.ReceiveFrom(connectionString);
            sbc.UseControlBus();
            sbc.SetConcurrentConsumerLimit(NHOPSvcSettings.Default.ConsumerLimit);
            sbc.SetDefaultSerializer<JsonMessageSerializer>();
            sbc.UseLog4Net();
          });
        }
        catch (Exception ex)
        {
          log.IfErrorFormat(ex, "Service bus creation error. Service bus uri is {0}", connectionString);
        }
      }
      else
      {
        log.IfErrorFormat("Service Bus URI is blank.");
      }
    }
  }
}
