using RdKafka;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestUtility
{
    public class RdKafkaDriver
    {
        public Producer kafkaProducer;
        public RdKafkaDriver()
        {
            var appConfig = new TestConfig(); 
            Log.Info($"Kafka Server: {appConfig.kafkaServer} ",Log.ContentType.URI);
            kafkaProducer = new Producer(appConfig.kafkaServer);
        }

        /// <summary>
        /// Send a message to kafka. 
        /// </summary>
        /// <param name="topicName">Kafka topic name e.g  VSS.VisionLink.Interfaces.Events.Telematics.Machine.SwitchStateEvent </param>
        /// <param name="message">Kafka Message</param>
        public void SendKafkaMessage(string topicName, string message)
        { 
            try
            {
                using (var topic = kafkaProducer.Topic(topicName))
                {
                    Log.Info($"Publish: {topicName} Message: {message} ", Log.ContentType.KafkaSend);
                    Console.WriteLine($"Publish: {topicName} Message: {message} ");
                    var data = Encoding.UTF8.GetBytes(message);
                    var deliveryReport = topic.Produce(data);                
                    var response = deliveryReport.ContinueWith(task =>
                    {
                        Log.Info($"Partition: {task.Result.Partition}, Offset: {task.Result.Offset} Incontinue: {deliveryReport.Status.ToString()}", Log.ContentType.KafkaResponse);
                    });
                    response.Wait();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, Log.ContentType.Error);
            }
        }
    }
}
