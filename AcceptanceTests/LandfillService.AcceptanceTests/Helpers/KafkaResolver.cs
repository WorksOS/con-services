using System;
using System.Collections.Generic;
using System.Threading;
using VSS.Kafka.DotNetClient;
using VSS.Kafka.DotNetClient.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using VSS.Kafka.DotNetClient.Producer;

namespace LandfillService.AcceptanceTests.Helpers
{
    public static class KafkaResolver
    {
        public static BinaryProducer Producer { get; set; }
        public static string KafkaTopic { get; set; }
        private static readonly Dictionary<string, BinaryProducer> kafkaTopics;
        private static readonly IRestProxySettings settings;

        static KafkaResolver()
        {
            kafkaTopics = new Dictionary<string, BinaryProducer>();
            if (settings == null)
                { settings = new DefaultRestProxySettings();}
        }


        /// <summary>
        /// Try and resolved the kafka topic. If it can't be found the add it to the dictionary 
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        public static BinaryProducer ResolveTopic(string topicName)
        {
            try
            {
                BinaryProducer producer;
                if (kafkaTopics.TryGetValue(topicName, out producer))
                {
                    return producer;
                }
                //Log.Write(" Create new topic producer " + topicName);
                producer = new BinaryProducer(topicName, MessageFormat.Binary, settings);
                kafkaTopics.Add(topicName, producer);
                return producer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error : " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Send the message to Kafka
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="kafkaInstance"></param>
        /// <param name="inMessage"></param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public static BinaryProducer SendMessage(string topic, BinaryProducer kafkaInstance, string inMessage, string assetId)
        {

            Thread.Sleep(50);
          //  Log.Write("Send message: " + inMessage);
            kafkaInstance.PublishToBatch(new VSS.Kafka.DotNetClient.Model.Message[]{new VSS.Kafka.DotNetClient.Model.Message() {
                Key = assetId,
                Value = inMessage
                 } });            
            return kafkaInstance;
        }
    }
}
