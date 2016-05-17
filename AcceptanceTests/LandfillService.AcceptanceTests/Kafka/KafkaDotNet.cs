using System;
using System.Threading;
using System.Collections.Generic;
using System.Configuration;
using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;
using Newtonsoft.Json;

namespace LandfillService.AcceptanceTests.LandFillKafka
{
    public class KafkaDotNet : IKafkaDriver
    {
        private string endpoint;
        private KafkaOptions options;
        private Producer producer;

        public KafkaDotNet()
        {
            endpoint = Config.KafkaEndpoint;
            options = new KafkaOptions(new Uri(endpoint));
            producer = new Producer(new BrokerRouter(options))
            {
                BatchSize = 1,
                BatchDelayTime = TimeSpan.FromMilliseconds(10)
            };
        }

        public string SendMessage(string topic, string message)
        {
            var result = producer.SendMessageAsync(topic, new[] { new Message(message) }).Result;
            Thread.Sleep(500);
            return JsonConvert.SerializeObject(result);
        }
    }
}
