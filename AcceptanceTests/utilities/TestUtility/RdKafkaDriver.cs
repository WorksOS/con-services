using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestUtility
{
  public class RdKafkaDriver
  {
    public Producer kafkaProducer;

    public RdKafkaDriver()
    {
      var appConfig = new TestConfig();
      Log.Info($"Kafka Server: {appConfig.kafkaServer} ", Log.ContentType.URI);
      var producerConfig = new Dictionary<string, object>
      {
        {"bootstrap.servers", appConfig.kafkaServer},
        {"session.timeout.ms", "10000"},
        {"retries", "3"},
        //{"batch.size", "1048576"},
        //{"linger.ms", "20"},
        //{"acks", "all"},
        //{"block.on.buffer.full", "true"}
      };

      kafkaProducer = new Producer(producerConfig);
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
        Log.Info($"Publish: {topicName} Message: {message} ", Log.ContentType.KafkaSend);
        Console.WriteLine($"Publish: {topicName} Message: {message} ");
        var data = Encoding.UTF8.GetBytes(message);
        var key = Encoding.UTF8.GetBytes(message);
        var deliveryReport = kafkaProducer.ProduceAsync(topicName, key, data).ContinueWith(task =>
        {
          Log.Info(
            $"Partition: {task.Result.Partition}, Offset: {task.Result.Offset} Incontinue: {task.Status.ToString()}",
            Log.ContentType.KafkaResponse);
          return 1;
        }).Result;

      }
      catch (Exception ex)
      {
        Log.Error(ex, Log.ContentType.Error);
      }
    }
  }
}
