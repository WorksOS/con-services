using System;
using System.Configuration;
using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;

namespace LandfillService.AcceptanceTests.Helpers
{
    public class Kafka
    {
        public string KafkaEndpoint;
        public ProduceResponse KafkaResponse;
        private KafkaOptions kafkaOptions;
        private Producer kafkaProducer;

        // Kafka is Singleton
        private static Kafka instance;
        private Kafka() 
        {
            KafkaEndpoint = ConfigurationManager.AppSettings["KafkaEndpoint"];
            kafkaOptions = new KafkaOptions(new Uri(KafkaEndpoint));
            kafkaProducer = new Producer(new BrokerRouter(kafkaOptions))
            {
                BatchSize = 1,
                BatchDelayTime = TimeSpan.FromMilliseconds(10)
            };
        }
        public static Kafka Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new Kafka();
                }
                return instance;
            }
        }

        /// <summary>
        /// Get Kafka producer
        /// </summary>
        /// <returns></returns>
        public Producer GetProducer()
        {
            return kafkaProducer;
        }
    }
}
