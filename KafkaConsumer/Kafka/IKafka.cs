using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.GenericConfiguration;
using VSS.Project.Service.Utils;

namespace KafkaConsumer.Kafka
{
    public interface IKafka
    {
        string ConsumerGroup { get; set; }
        string OffsetReset { get; set; }
        string Uri { get; set; }
        bool EnableAutoCommit { get; set; }
        int Port { get; set; }
        void Subscribe(List<string> topics);
        void Commit();
        void InitConsumer(IConfigurationStore configurationStore, string groupName = null);
        void InitProducer(IConfigurationStore configurationStore);
        void Send(string topic, IEnumerable<KeyValuePair<string, string>> messagesToSendWithKeys);
        void Send(IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> topicMessagesToSendWithKeys);
        Message Consume(TimeSpan timeout);
        void Dispose();
        bool IsInitializedProducer { get; }
        bool IsInitializedConsumer { get; }
        Task Send(string topic, KeyValuePair<string, string> messageToSendWithKey);
    }

    public class Message
    {
        public IEnumerable<byte[]> payload { get; }
        public Error message { get; }

        public Message(IEnumerable<byte[]> payload, Error message)
        {
            this.payload = payload;
            this.message = message;
        }
    }

    public enum Error
    {
        NO_ERROR,
        NO_DATA
    }
}
