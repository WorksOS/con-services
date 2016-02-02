using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using Autofac;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net;
using Topshelf;
using Topshelf.Runtime;
using VSP.MasterData.Common.KafkaWrapper;
using VSP.MasterData.Common.KafkaWrapper.Interfaces;
using VSP.MasterData.Common.KafkaWrapper.Models;
using VSP.MasterData.Customer.Data;
using VSS.MasterData.Customer.Processor.Handler;
using VSS.MasterData.Customer.Processor.Interfaces;
using IContainer = Autofac.IContainer;

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
            builder.RegisterType<CustomerEventHandler>().AsSelf().SingleInstance();

            string kafkaUri = GetKafkaEndPointUri(ConfigurationManager.AppSettings["TopicName"]);

            builder.Register(
                c => new KafkaConsumerParams(
                    ConfigurationManager.AppSettings["ConsumerGroupName"],
                    kafkaUri,
                    ConfigurationManager.AppSettings["TopicName"])).As<KafkaConsumerParams>();

            builder.RegisterType<CustomerDataService>().As<ICustomerDataService>().SingleInstance();

            builder.Register(config =>
            {
                var eAgg = new EventAggregator();
                eAgg.Subscribe(config.Resolve<CustomerEventHandler>(
                    new NamedParameter("customerDataService", config.Resolve<ICustomerDataService>())));
                return eAgg;
            })
            .As<IEventAggregator>().SingleInstance(); ;

            builder.RegisterType<ConsumerWrapper>().As<IConsumerWrapper>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerProcessor>().As<ICustomerProcessor>().InstancePerLifetimeScope();

            Container = builder.Build();
            return Container.Resolve<ServiceController>();

        }

        private static string GetKafkaEndPointUri(string kafkaTopicName)
        {
          return ConfigurationManager.AppSettings["KafkaServerUri"];
            /*try
            {
                string jsonStr;
                using (var wc = new WebClient())
                {
                    jsonStr = wc.DownloadString(new Uri(ConfigurationManager.AppSettings["DicoveryServiceUri"]));
                }
                JObject jsonObj = JObject.Parse(jsonStr);
                var token = jsonObj.SelectToken("$.Topics[?(@.Name == '" + kafkaTopicName + "')].URL");
                return token.ToString();
            }
            catch (Exception)
            {

            }
            return null;*/
        }
    }
}
