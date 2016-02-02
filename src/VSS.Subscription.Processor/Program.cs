using Autofac;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Runtime;
using VSP.MasterData.Common.KafkaWrapper;
using VSP.MasterData.Common.KafkaWrapper.Interfaces;
using VSP.MasterData.Common.KafkaWrapper.Models;
using VSS.Subscription.Data.MySql;
using VSS.Subscription.Model.Interfaces;
using VSS.Subscription.Processor.Interfaces;

namespace VSS.Subscription.Processor
{
    class Program
    {
        protected static IContainer Container { get; set; }
        public static void Main(string[] args)
        {
            TopshelfExitCode exitCode = HostFactory.Run(c =>
            {
                c.SetServiceName("_MasterDataSubscriptionConsumer");
                c.SetDisplayName("_MasterDataSubscriptionConsumer");
                c.SetDescription("Service for processing subscription payloads from Kafka");
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
                //c.UseLog4Net();
            });
        }


        private static ServiceController ServiceFactory(HostSettings settings)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceController>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<SubscriptionEventHandler>().AsSelf().SingleInstance();

            string kafkaUri = GetKafkaEndPointUri(Settings.Default.TopicName);

            builder.Register(
                c => new KafkaConsumerParams(
                    Settings.Default.ConsumerGroupName,
                    kafkaUri,
                    Settings.Default.TopicName)).As<KafkaConsumerParams>();

            builder.RegisterType<MySqlSubscriptionService>().As<ISubscriptionService>().SingleInstance();

            builder.Register(config =>
            {
                var eAgg = new EventAggregator();
                eAgg.Subscribe(config.Resolve<SubscriptionEventHandler>(
                    new NamedParameter("subscriptionService", config.Resolve<ISubscriptionService>())));
                return eAgg;
            })
            .As<IEventAggregator>().SingleInstance(); ;

            builder.RegisterType<ConsumerWrapper>().As<IConsumerWrapper>().InstancePerLifetimeScope();
            builder.RegisterType<SubscriptionEventConsumer>().As<IConsumer>().InstancePerLifetimeScope();

            Container = builder.Build();
            return Container.Resolve<ServiceController>();

        }


        private static string GetKafkaEndPointUri(string kafkaTopicName)
        {
          return Settings.Default.KafkaServerUri;
          /*try
            {
                string jsonStr;
                using (var wc = new WebClient())
                {
                    jsonStr = wc.DownloadString(new Uri(Settings.Default.DicoveryServiceUri));
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
