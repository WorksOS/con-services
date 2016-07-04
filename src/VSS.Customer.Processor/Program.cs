using System.Configuration;
using Autofac;
using log4net;
using System;
using System.Reflection;
using org.apache.kafka.clients.consumer;
using Topshelf;
using Topshelf.Runtime;
using VSS.Customer.Data;
using VSS.Customer.Data.Interfaces;
using VSS.MasterData.Common.Processor;

namespace VSS.Customer.Processor
{
  internal class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static IContainer Container { get; set; }

    public static void Main(string[] args)
    {
      TopshelfExitCode exitCode = HostFactory.Run(c =>
      {
        c.SetServiceName("_CustomerMasterDataConsumer");
        c.SetDisplayName("_CustomerMasterDataConsumer");
        c.SetDescription("Service for processing customer master data payloads from Kafka");

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
          s.WhenStarted(o => { o.Start(); });
          s.WhenStopped(o => { o.Stop(); });
        });
      });
    }

    private static ServiceController ServiceFactory(HostSettings settings)
    {
      Log.Debug("CustomerProcessor: starting ServiceFactory");

      var builder = new ContainerBuilder();
      builder.RegisterType<ServiceController>()
        .AsSelf()
        .SingleInstance();

      string confluentBaseUrl = ConfigurationManager.AppSettings["KafkaUri"];
      if (string.IsNullOrWhiteSpace(confluentBaseUrl))
        throw new ArgumentNullException("RestProxy Base Url is empty");

      builder.RegisterType<VSS.MasterData.Common.Processor.Processor>().As<IProcessor>().SingleInstance();
      builder.RegisterType<CustomerEventObserver>().As<IObserver<ConsumerRecord>>().SingleInstance();
      builder.RegisterType<MySqlCustomerRepository>().As<ICustomerService>().SingleInstance();

      Container = builder.Build();

      return Container.Resolve<ServiceController>();
    }
  }
}
