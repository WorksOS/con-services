using System;
using System.Threading;
using System.Collections.Generic;
using System.Configuration;
using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;
using Newtonsoft.Json;

namespace VSP.MasterData.Project.AcceptanceTests.Kafka
{
    public class KafkaDotNet
    {
        private static string endpoint;
        private static KafkaOptions options;
        private static Producer producer;

        static KafkaDotNet()
        {
            endpoint = Config.KafkaEndpoint;
            options = new KafkaOptions(new Uri(endpoint));
            producer = new Producer(new BrokerRouter(options))
            {
                BatchSize = 1,
                BatchDelayTime = TimeSpan.FromMilliseconds(10)
            };
        }

        public static string SendMessage(string topic, string message)
        {
            var result = producer.SendMessageAsync(topic, new[] { new Message(message) }).Result;
            Thread.Sleep(30000);
            return JsonConvert.SerializeObject(result);
        }
    }
}
