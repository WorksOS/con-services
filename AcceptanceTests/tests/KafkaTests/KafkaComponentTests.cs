using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RdKafka;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KafkaTests
{
  [TestClass]
  public class KafkaComponentTests
  {
    private string kafkaUrl = null;
    private string kafkaPort = null;
    private string kafkaUri = null;
    private string kafkaTopic = null;
    private List<string> kafkaTopics = new List<string>();
    private string kafkaGroupName = null;
    private TimeSpan timeout;


    [TestInitialize]
    public void InitTest()
    {
      kafkaUrl = "localhost";
      kafkaPort = "9092";
      kafkaUri = string.Format("{0}:{1}", kafkaUrl, kafkaPort);
      kafkaTopic = "streams-console-input";
      kafkaGroupName = "whatever";
      timeout = TimeSpan.FromSeconds(60);
    }

    [TestMethod] 
    public void CanReadTestStreamingEventFromKafka()
    {
      //var initialMessage = ReadMessageToKafka(kafkaTopic);
        
      var toWrite = "HelloWorldOfJeannie";

      string message = JsonConvert.SerializeObject(toWrite,
        new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

      var ret = SendMessageToKafka(kafkaTopic, message);

      var messageRead = ReadMessageToKafka(kafkaTopic);      
    }
    


    private string ReadMessageToKafka(string topicName)
    {
      kafkaTopics.Clear();
      kafkaTopics.Add(topicName);
      string message = null;
      Consumer consumer = null;

      try
      {

        var topicConfig = new TopicConfig();
        topicConfig["auto.offset.reset"] = "earliest"; //  "latest";

        Config conf = new Config()
        {
          GroupId = kafkaGroupName,
          EnableAutoCommit = false,
          DefaultTopicConfig = topicConfig
        };
        consumer = new Consumer(conf);
        consumer.Subscribe(kafkaTopics);
        var result = consumer.Consume(timeout);
        if (result.HasValue)
          if (result.Value.Error == ErrorCode.NO_ERROR)
            message = result.Value.Message.Payload.ToString();
          else
            message = "error";
        else
          message = "noData";
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
      finally
      {
        if (consumer != null)
          consumer.Unsubscribe();
        if (consumer != null)
          consumer.Dispose();
      }
      return message;
    }
    private int SendMessageToKafka(string topicName, string message)
    {
      kafkaTopics.Clear();
      kafkaTopics.Add(topicName);

      try
      {
        using (Producer producer = new Producer(kafkaUri))
        using (Topic topic = producer.Topic(topicName))
        {
          byte[] data = Encoding.UTF8.GetBytes(message);
          Task<DeliveryReport> deliveryReport = topic.Produce(data);
          deliveryReport.Wait(10000);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
      return 1;
    }
  }
}
