using Autofac;
using System.Configuration;
using System.Net;
using Topshelf;
using Topshelf.Runtime;
//using VSP.MasterData.Common.KafkaWrapper;
//using VSP.MasterData.Common.KafkaWrapper.Interfaces;
//using VSP.MasterData.Common.KafkaWrapper.Models;
using VSP.MasterData.Customer.Data;
using VSS.Kafka.DotNetClient;
using VSS.Kafka.DotNetClient.Consumer;
using VSS.Kafka.DotNetClient.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using VSS.MasterData.Customer.Processor.Handler;
using VSS.MasterData.Customer.Processor.Interfaces;


namespace VSS.MasterData.Customer.Processor
{
  class Program
  {
    private static IContainer Container { get; set; }
    static void Main(string[] args)
    {
      var program = new Program();
      program.Run();
    }
    private void Run()
    {
      var exitCode = HostFactory.Run(c =>
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
      var builder = new ContainerBuilder();
      builder.RegisterType<ServiceController>()
          .AsSelf()
          .SingleInstance();
      //builder.RegisterGeneric<CustomerEventHandler>().As<IConsumerObserver<<T>>.SingleInstance();
      // builder.RegisterType<CustomerEventHandler>().As<IConsumerObserver<IEnumerable<KafkaBinaryConsumerMessage>>>().SingleInstance();
      //string kafkaUri = GetKafkaEndPointUri(ConfigurationManager.AppSettings["TopicName"]);

      //builder.Register(
      //    c => new KafkaConsumerParams(
      //        ConfigurationManager.AppSettings["ConsumerGroupName"],
      //        kafkaUri,
      //        ConfigurationManager.AppSettings["TopicName"])).As<KafkaConsumerParams>();

      builder.RegisterType<CustomerDataService>().As<ICustomerDataService>().SingleInstance();

      //builder.Register(config =>
      //{
      //    var eAgg = new EventAggregator();
      //    eAgg.Subscribe(config.Resolve<CustomerEventHandler>(
      //        new NamedParameter("customerDataService", config.Resolve<ICustomerDataService>())));
      //    return eAgg;
      //})
      //.As<IEventAggregator>().SingleInstance(); ;

      //builder.RegisterType<ConsumerWrapper>().As<IConsumerWrapper>().InstancePerLifetimeScope();

      #region RPL

      builder.Register<CreateConsumerRequest>(config =>
      {
        CreateConsumerRequest consumerRequest = new CreateConsumerRequest();
        consumerRequest.Topic = ConfigurationManager.AppSettings["TopicName"];
        consumerRequest.GroupName = ConfigurationManager.AppSettings["ConsumerGroupName"];
        consumerRequest.MessageFormat = MessageFormat.Binary;
        consumerRequest.InstanceId = Dns.GetHostName();

        return consumerRequest;
      }).As<CreateConsumerRequest>().InstancePerLifetimeScope();

      builder.RegisterType<CustomerProcessor>().As<ICustomerProcessor>().InstancePerLifetimeScope();

      builder.RegisterType<ObserverHandler>().As<IObserverHandler>();
      // Register the settings
      builder.RegisterType<AppConfigSettings>().As<IRestProxySettings>();

      // Register the Consumer
      builder.Register<BinaryConsumer>(ctx =>
      {
        var isettings = ctx.Resolve<IRestProxySettings>();
        return new BinaryConsumer(isettings);
      }).AsImplementedInterfaces().AsSelf();

      // Register the Observers
      builder.RegisterType<CustomerEventHandler>().AsImplementedInterfaces().AsSelf();
      #endregion

      Container = builder.Build();
      return Container.Resolve<ServiceController>();

    }
  }
}
