using RdKafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.GenericConfiguration;

namespace KafkaConsumer.Kafka
{
    public class RdKafkaDriver : IKafka
    {
        Consumer rdConsumer = null;
        Producer rdProducer = null;

        private readonly Object syncPollObject = new object();
        private Config config;


        public string ConsumerGroup { get; set; }

        public string Uri { get; set; }

        public string OffsetReset { get; set; }

        public bool EnableAutoCommit { get; set; }

        public int Port { get; set; }

        public bool IsInitializedProducer { get; private set; } = false;
        public bool IsInitializedConsumer { get; private set; } = false;

        public void Commit()
        {
            rdConsumer?.Commit();
        }

        public Message Consume(TimeSpan timeout)
        {
            lock (syncPollObject)
            {
                var result = rdConsumer.Consume(timeout);
                if (result.HasValue)
                    if (result.Value.Error == ErrorCode.NO_ERROR)
                        return new Message(new List<byte[]>() {result.Value.Message.Payload},
                            Error.NO_ERROR);
                    else
                        return new Message(null, (Error) (int) result.Value.Error);
                else
                    return new Message(null, Error.NO_DATA);
            }
        }

     
        public void InitConsumer(IConfigurationStore configurationStore, string groupName = null)
        {
            ConsumerGroup = groupName == null ? configurationStore.GetValueString("KAFKA_GROUP_NAME") : groupName;
            EnableAutoCommit = configurationStore.GetValueBool("KAFKA_AUTO_COMMIT").Value;
            OffsetReset = configurationStore.GetValueString("KAFKA_OFFSET");
            Uri = configurationStore.GetValueString("KAFKA_URI");
            Port = configurationStore.GetValueInt("KAFKA_PORT");

            Console.WriteLine("KAFKA_GROUP_NAME:" + ConsumerGroup);
            Console.WriteLine("KAFKA_AUTO_COMMIT:" + EnableAutoCommit);
            Console.WriteLine("KAFKA_OFFSET:" + OffsetReset);
            Console.WriteLine("KAFKA_URI:" + Uri);
            Console.WriteLine("KAFKA_PORT:" + Port);

            var topicConfig = new TopicConfig();
            topicConfig["auto.offset.reset"] = OffsetReset;

            config = new Config()
            {
                GroupId = ConsumerGroup,
                EnableAutoCommit = EnableAutoCommit,
                DefaultTopicConfig = topicConfig
            };
            IsInitializedConsumer = true;
        }

        public void Subscribe(List<string> topics)
        {
            rdConsumer = new Consumer(config, Uri);
            rdConsumer.Subscribe(topics);
        }


        public void InitProducer(IConfigurationStore configurationStore)
        {
            //overrideConfigValues = ConfigurationManager.GetSection("ikvmConsumerSettings") as NameValueCollection;

            Config config = new Config();
            var topicConfig = new TopicConfig();

/*            foreach (string key in overrideConfigValues)
            {
                string value = overrideConfigValues[key];
                if (key != "receive.buffer.bytes" && key != "zookeeper.session.timeout.ms" && key != "offsets.commit.timeout.ms" &&
                    key != "fetch.max.wait.ms" && key != "auto.offset.reset" && key != "key.deserializer" &&
                    key != "value.deserializer" && key != "key.serializer" && key != "value.serializer" &&
                    key != "request.timeout.ms")
                    config[key] = value;
            }


            // topicConfig["request.required.acks"] = "0";

            // config["socket.blocking.max.ms"] = "1";

            config["queue.buffering.max.messages"] = "200";
            config["queue.buffering.max.ms"] = "50";*/

            config.DefaultTopicConfig = topicConfig;

            //socket.blocking.max.ms=1
            rdProducer = new Producer(config, configurationStore.GetValueString("KAFKA_URI"));
            IsInitializedProducer = true;
        }

        public void Send(string topic, IEnumerable<KeyValuePair<string, string>> messagesToSendWithKeys)
        {
            Console.WriteLine();
            List<Task> tasks = new List<Task>();
            using (Topic myTopic = rdProducer.Topic(topic))
            {
                foreach (var messagesToSendWithKey in messagesToSendWithKeys)
                {
                    byte[] data = Encoding.UTF8.GetBytes(messagesToSendWithKey.Value);
                    byte[] key = Encoding.UTF8.GetBytes(messagesToSendWithKey.Key);
                    tasks.Add(myTopic.Produce(data, key));
                }
            }
            Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(4));
        }


        public void Send(IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> topicMessagesToSendWithKeys)
        {
            foreach (var topicMessagesToSendWithKey in topicMessagesToSendWithKeys)
            {
                using (Topic myTopic = rdProducer.Topic(topicMessagesToSendWithKey.Key))
                {
                    byte[] data = Encoding.UTF8.GetBytes(topicMessagesToSendWithKey.Value.Value);
                    byte[] key = Encoding.UTF8.GetBytes(topicMessagesToSendWithKey.Value.Key);
                    DeliveryReport deliveryReport = myTopic.Produce(data, key).Result;
                }
            }
        }


        public void Dispose()
        {
            lock (syncPollObject)
            {
                rdConsumer?.Unsubscribe();
                rdConsumer?.Dispose();
            }
            rdProducer?.Dispose();
            IsInitializedProducer = false;
            IsInitializedConsumer = false;

        }
    }
}
