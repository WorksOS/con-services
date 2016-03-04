using System.Configuration;
using KafkaNet.Protocol;
using LandfillService.AcceptanceTests.Interfaces;

namespace LandfillService.AcceptanceTests.Helpers
{
    /// <summary>
    /// MessageFactory is Singleton (just in case it needs be derived)
    /// </summary>
    public class MessageFactory
    {
        private static MessageFactory instance;
        private MessageFactory() { }
        public static MessageFactory Instance
        {
            get { return instance ?? (instance = new MessageFactory()); }
        }

        /// <summary>
        /// Generates IMessage that can be Send() to a Kafka queue. Data source comes from SpecFlow feature file Table.
        /// </summary>
        /// <param name="messageStr"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public IMessage CreateMessage(string messageStr, MessageType eventType)
        {
            string topic = "";
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
            
            return new KafkaMessage(Kafka.Instance.GetProducer(), topic, new Message(messageStr));
        }
    }
}
