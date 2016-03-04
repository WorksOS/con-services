using System;
using System.Configuration;
using VSS.Kafka.DotNetClient.Helper;
using VSS.Messaging.Kafka.Interfaces;


namespace LandfillService.AcceptanceTests.Helpers
{
    public static class KafkaRpl
    {
        private static IProducerShareableConfigurator pconfigurator; 
        private static IProducerShareable producer;

        public static void InitialiseKafkaRpl()
        {
            pconfigurator = new ProducerShareableConfigurator();
            producer = new ProducerShareable(pconfigurator);
            producer.GetConfigurator(context => { context.BaseUrl = ConfigurationManager.AppSettings["RestProxyBaseUrl"]; });
        }

        public static string GetMyTopic(MessageType eventType)
        {
            var topic = string.Empty;
            switch (eventType)
            {
                case MessageType.CreateProjectEvent:
                case MessageType.DeleteProjectEvent:
                case MessageType.UpdateProjectEvent:
                    topic = ConfigurationManager.AppSettings["ProjectMasterDataTopic"];
                    break;
                case MessageType.CreateCustomerEvent:
                case MessageType.AssociateProjectCustomerEvent:
                    topic = ConfigurationManager.AppSettings["CustomerMasterDataTopic"];
                    break;
                case MessageType.CreateCustomerSubscriptionEvent:
                case MessageType.CreateProjectSubscriptionEvent:
                case MessageType.UpdateCustomerSubscriptionEvent:
                case MessageType.UpdateProjectSubscriptionEvent:
                case MessageType.DissociateProjectSubscriptionEvent:
                case MessageType.AssociateProjectSubscriptionEvent:
                    topic = ConfigurationManager.AppSettings["SubscriptionTopic"];
                    break;
                case MessageType.AssociateCustomerUserEvent:
                    topic = ConfigurationManager.AppSettings["CustomerUserMasterDataTopic"];
                    break;
            }
            return topic;
        }            

        public static void SendToKafka(string topicName, string message)
        {
            object[] kafkaData = {message};
            var result = producer.PublishSync(topicName, new KafkaBinaryConsumerMessagePartition { Key = "somekey", Partition = 0 }, kafkaData);
            Console.WriteLine(((KafkaPublishResponse)result).Status);
        }

    }
}
