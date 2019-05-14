using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestUtility
{
  public class RdKafkaDriver
  {
    public Producer<byte[], byte[]> kafkaProducer;

    public RdKafkaDriver()
    {
      var appConfig = new TestConfig();
      Console.WriteLine($"Kafka Server: {appConfig.kafkaServer} ");
      var producerConfig = new Dictionary<string, string>
      {
        {"bootstrap.servers", appConfig.kafkaServer},
        {"session.timeout.ms", "10000"},
        {"retries", "3"},
        //{"batch.size", "1048576"},
        //{"linger.ms", "20"},
        //{"acks", "all"},
        //{"block.on.buffer.full", "true"}
      };

      kafkaProducer = new Producer<byte[], byte[]>(producerConfig.ToList());
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
        Console.WriteLine($"Publish: {topicName} Message: {message} ");
        var data = Encoding.UTF8.GetBytes(message);
        var key = Encoding.UTF8.GetBytes(message);
        var deliveryReport = kafkaProducer.ProduceAsync(topicName, new Message<byte[], byte[]> {Key = key, Value = data}).ContinueWith(task =>
        {
          Console.WriteLine(
            $"Partition: {task.Result.Partition}, Offset: {task.Result.Offset} Incontinue: {task.Status.ToString()}");
          return 1;
        }).Result;

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
    }
  }
}
