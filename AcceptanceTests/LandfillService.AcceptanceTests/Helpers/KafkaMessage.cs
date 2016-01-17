using System.Collections.Generic;
using KafkaNet;
using KafkaNet.Protocol;
using Newtonsoft.Json;

namespace LandfillService.AcceptanceTests.Helpers
{
    /// <summary>
    /// KafkaMessage that can be Send()
    /// </summary>
    public class KafkaMessage : IMessage 
    {
       // private static readonly ILog log = LogManager.GetLogger(typeof(KafkaMessage));

        private Producer _producer;
        private string _topic;
        private Message _message;

        public KafkaMessage(Producer producer, string topic, Message message)
        {
            _producer = producer;
            _topic = topic;
            _message = message;
        }

        /// <summary>
        /// GetMessageType()
        /// </summary>
        /// <returns></returns>
        public MessageType GetMessageType()
        {
            //if (_topic == UtilizationServicesConfig.InternalQueueTopic)
            //    return MessageType.InternalQueue;
            //if (_topic == UtilizationServicesConfig.HoursTopic)
            //    return MessageType.HoursEvent;
            //if (_topic == UtilizationServicesConfig.EngineOperatingStatusTopic)
            //    return MessageType.EngineOperatingStatusEvent;
            //if (_topic == UtilizationServicesConfig.MovingTopic)
            //    return MessageType.MovingEvent;
            //if (_topic == UtilizationServicesConfig.SwitchTopic)
            //    return MessageType.SwitchStateEvent;
            //if (_topic == UtilizationServicesConfig.WorkDefinitionTopic)
            //    return MessageType.WorkDefinition;
            //if (_topic == UtilizationServicesConfig.OdometerTopic)
            //    return MessageType.OdometerEvent;

            return MessageType.Invalid;
        }

        /// <summary>
        /// Send KafkaMessage to respective queues. If we are triggering the internal queue,
        /// then the wait is also done here (this is the single place we apply wait).
        /// </summary>
        /// <returns></returns>
        public object Send()
        {
            var result = _producer.SendMessageAsync(_topic, new[] { _message }).Result;

            // Logging
            //log.Info(LogFormatter.Format(JsonConvert.SerializeObject(result as List<ProduceResponse>, Formatting.None),
            //    LogFormatter.ContentType.KafkaProduceResponse));

            return result;
        }
    }
}
