using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using java.util;
using log4net;
using System;
using System.Reflection;
using org.apache.kafka.clients.consumer;
using VSS.Customer.Data.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using VSS.Customer.Processor.Consumer;
using VSS.Customer.Processor.Interfaces;
using Random = System.Random;

namespace VSS.Customer.Processor
{
  public class CustomerProcessor : ICustomerProcessor
  {
    private readonly IObserver<ConsumerInstanceResponse> _observer;
    private readonly CustomerEventObserver _subscriber;
    private readonly ConsumerWrapper _consumerWrapper;
    private readonly KafkaConsumer javaConsumer;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public CustomerProcessor(ICustomerService service, IConsumerConfigurator configurator)
    {
      try
      {
        log4net.Config.XmlConfigurator.Configure();

        var random = new Random();

        var props = new Properties();
        props.put("bootstrap.servers", Settings.Default.KafkaUri);
        props.put("client.id", random.Next().ToString());
        props.put("group.id", Settings.Default.ConsumerGroupName);
        props.put("enable.auto.commit", "true");
        props.put("auto.commit.interval.ms", "1000");
        props.put("session.timeout.ms", "30000");
        props.put("key.deserializer", "org.apache.kafka.common.serialization.StringDeserializer");
        props.put("value.deserializer", "org.apache.kafka.common.serialization.StringDeserializer");
        javaConsumer = new KafkaConsumer(props);

        _subscriber = new CustomerEventObserver(service);
      }
      catch (Exception error)
      {
        Log.Error("Error creating the consumer" + error.Message + error.StackTrace, error);
      }
    }

    public void Process()
    {
      javaConsumer.subscribe(Arrays.asList(Settings.Default.TopicName));
      var consumingThread = new Thread(JavaConsumerWorker);
      consumingThread.Start();   
    }

    private void JavaConsumerWorker()
    {
      var buffer = new List<ConsumerInstanceResponse>();
      const int minBatchSize = 1;

      while (true)
      {
        var records = javaConsumer.poll(100);
        buffer.AddRange(records.Cast<ConsumerInstanceResponse>());

        if (buffer.Count < minBatchSize) continue;

          foreach (var consumerRecord in buffer)
          {
            _subscriber.OnNext(consumerRecord);
          }

        try
        {
          javaConsumer.commitSync();
        }
        catch (CommitFailedException cfe)
        {
          // See: https://mail-archives.apache.org/mod_mbox/kafka-users/201601.mbox/%3CCAJDuW=Da9L0GLEp0WVFVpE0DnhaWH59xBCs2dN8yVsORKnFPGA@mail.gmail.com%3E
          Console.WriteLine("Consider reducing size of processed batch: {0}", cfe);
        }

        buffer.Clear();
      }

    }

    public void Stop()
    {
      javaConsumer.close();
    }
  }
}
