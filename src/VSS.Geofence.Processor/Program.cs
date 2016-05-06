using System;
using System.Reflection;
using Autofac;
using log4net;
using org.apache.kafka.clients.consumer;
using Topshelf;
using Topshelf.Runtime;
using VSS.Geofence.Data;
using VSS.Geofence.Data.Interfaces;
using VSS.Geofence.Processor.Interfaces;
using VSS.Geofence.Processor.Properties;

namespace VSS.Geofence.Processor
{
  internal class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    protected static IContainer Container { get; set; }

    public static void Main(string[] args)
    {
      TopshelfExitCode exitCode = HostFactory.Run(c =>
      {
        c.SetServiceName("_MasterDataGeofenceConsumer");
        c.SetDisplayName("_MasterDataGeofenceConsumer");
        c.SetDescription("Service for processing Geofence payloads from Kafka");
        c.RunAsLocalSystem();
        c.StartAutomatically();
        c.EnableServiceRecovery(cfg =>
        {
          cfg.RestartService(1);
          cfg.RestartService(1);
          cfg.RestartService(1);
        });
        c.Service<ServiceController>(s =>
        {
          s.ConstructUsing(ServiceFactory);
          s.WhenStarted(o => o.Start());
          s.WhenStopped(o => o.Stop());
        });
        // c.UseLog4Net();
      });
    }


    private static ServiceController ServiceFactory(HostSettings settings)
    {
      Log.Debug("GeofenceProcessor: starting ServiceFactory");

      var builder = new ContainerBuilder();
      builder.RegisterType<ServiceController>()
        .AsSelf()
        .SingleInstance();

      string confluentBaseUrl = Settings.Default.KafkaUri; //["RestProxyBaseUrl"];
      if (string.IsNullOrWhiteSpace(confluentBaseUrl))
        throw new ArgumentNullException("RestProxy Base Url is empty");

      builder.RegisterType<GeofenceProcessor>().As<IGeofenceProcessor>().SingleInstance();
      builder.RegisterType<GeofenceEventObserver>().As<IObserver<ConsumerRecord>>().SingleInstance();
      builder.RegisterType<MySqlGeofenceRepository>().As<IGeofenceService>().SingleInstance();

      Container = builder.Build();

      return Container.Resolve<ServiceController>();
    }
  }
}
