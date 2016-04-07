using System;
using KafkaNet;
using KafkaNet.Protocol;
using LandfillService.AcceptanceTests.Interfaces;

namespace LandfillService.AcceptanceTests.LandFillKafka
{
    /// <summary>
    /// KafkaMessage that can be Send()
    /// </summary>
    public class KafkaMessage : IMessage 
    {
        private readonly Producer producer;
        private readonly string topic;
        private readonly Message message;

        public KafkaMessage(Producer producer, string topic, Message message)
        {
            this.producer = producer;
            this.topic = topic;
            this.message = message;
        }

        /// <summary>
        /// Send KafkaMessage to respective queues. If we are triggering the internal queue,
        /// then the wait is also done here (this is the single place we apply wait).
        /// </summary>
        /// <returns></returns>
        public object Send()
        {
            try
            {
                var result = producer.SendMessageAsync(topic, new[] { message }).Result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex;
            }
        }
    }
}
