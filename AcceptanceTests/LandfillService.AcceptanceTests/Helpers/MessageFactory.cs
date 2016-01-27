using System;
using System.Configuration;
using KafkaNet.Protocol;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using LandfillService.AcceptanceTests.Models;
using LandfillService.AcceptanceTests.Models.KafkaTopics;

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
            topic = ConfigurationManager.AppSettings["AssetMasterDataTopic"];
            return new KafkaMessage(Kafka.Instance.GetProducer(), topic, new Message(messageStr));
        }
    }
}
