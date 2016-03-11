using Autofac;
using log4net;
using System;
using System.Configuration;
using System.Net;
using System.Reflection;
using Topshelf;
using Topshelf.Runtime;
using VSS.Kafka.DotNetClient.Model;
using VSS.Project.Data;
using VSS.Project.Data.Interfaces;
using VSS.Project.Processor.Consumer;
using VSS.Project.Processor.Interfaces;

namespace VSS.Project.Processor
{
  internal class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    protected static IContainer Container { get; set; }

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
      
      string confluentBaseUrl = ConfigurationManager.AppSettings["KafkaServerUri"]; //["RestProxyBaseUrl"];
      if (string.IsNullOrWhiteSpace(confluentBaseUrl))
        throw new ArgumentNullException("RestProxy Base Url is empty");

      string kafkaTopicName = Settings.Default.TopicName;
      string consumerGroupName = Settings.Default.ConsumerGroupName;

      builder.Register(config =>
      {
        var consumerConfigurator = new ConsumerConfigurator(confluentBaseUrl, kafkaTopicName, consumerGroupName,
          Dns.GetHostName(), 1024);
        return consumerConfigurator;
      }).As<IConsumerConfigurator>().SingleInstance();

      builder.RegisterType<ProjectProcessor>().As<IProjectProcessor>().SingleInstance();

      builder.RegisterType<ProjectEventObserver>().As<IObserver<ConsumerInstanceResponse>>().SingleInstance();

      builder.RegisterType<MySqlProjectRepository>().As<IProjectService>().SingleInstance();

      Container = builder.Build();
      return Container.Resolve<ServiceController>();
    }
  }
}
