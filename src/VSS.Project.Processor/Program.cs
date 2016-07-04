using System.Configuration;
using Autofac;
using log4net;
using System;
using System.Reflection;
using org.apache.kafka.clients.consumer;
using Topshelf;
using Topshelf.Runtime;
using VSS.MasterData.Common.Processor;
using VSS.Project.Data;
using VSS.Project.Data.Interfaces;

namespace VSS.Project.Processor
{
  internal class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static IContainer Container { get; set; }

    public static void Main(string[] args)
    {
      TopshelfExitCode exitCode = HostFactory.Run(c =>
      {
        c.SetServiceName("_MasterDataProjectConsumer");
        c.SetDisplayName("_MasterDataProjectConsumer");
        c.SetDescription("Service for processing Project payloads from Kafka");
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
      Log.Debug("ProjectProcessor: starting ServiceFactory");
      
      var builder = new ContainerBuilder();
      builder.RegisterType<ServiceController>()
        .AsSelf()
        .SingleInstance();
      
      string confluentBaseUrl = ConfigurationManager.AppSettings["KafkaUri"]; 
      if (string.IsNullOrWhiteSpace(confluentBaseUrl))
        throw new ArgumentNullException("RestProxy Base Url is empty");

      builder.RegisterType<VSS.MasterData.Common.Processor.Processor>().As<IProcessor>().SingleInstance();
      builder.RegisterType<ProjectEventObserver>().As<IObserver<ConsumerRecord>>().SingleInstance();
      builder.RegisterType<MySqlProjectRepository>().As<IProjectService>().SingleInstance();

      Container = builder.Build();

      return Container.Resolve<ServiceController>();
    }
  }
}
