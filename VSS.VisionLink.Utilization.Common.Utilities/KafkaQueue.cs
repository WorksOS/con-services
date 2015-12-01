using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using KafkaNet;
using KafkaNet.Common;
using KafkaNet.Model;
using KafkaNet.Protocol;
using log4net;
using Newtonsoft.Json;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Landfill.Common.Interfaces;

namespace VSS.VisionLink.Landfill.Common.Utilities
{
  public class KafkaProjectEventQueue<T> : KafkaQueue<T>, IKafkaQueue<T> where T : IProjectEvent
  {
    public KafkaProjectEventQueue(string kafkaTopic, int offset = 0)
      : base(kafkaTopic, offset)
    {
    }
  }

  public class KafkaSubscriptionEventQueue<T> : KafkaQueue<T>, IKafkaQueue<T> where T : ISubscriptionEvent
  {
    public KafkaSubscriptionEventQueue(string kafkaTopic, int offset = 0)
      : base(kafkaTopic, offset)
    {
    }
  }


  public abstract class KafkaQueue<T>
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly Queue<Message> internalStack = new Queue<Message>();

    private readonly object lockObject = new object();
    private readonly Consumer consumer;
    private readonly AutoResetEvent semaphore;

    protected KafkaQueue(string kafkaTopic, int offset = 0)
    {
      //Load configuration from config file
      var kafkaUri = ConfigurationManager.AppSettings["KafkaServerUri"];
      var kafkaBackoff = ConfigurationManager.AppSettings["KafkaBackoffInterval"];

      var options = new KafkaOptions(new Uri(kafkaUri))
      {
        Log = new ConsoleLog()
      };

      //Fillup consumers
      OffsetPosition[] positions = {new OffsetPosition {PartitionId = 0, Offset = offset + 1}};
      consumer = new Consumer(new ConsumerOptions(kafkaTopic, new BrokerRouter(options))
      {
        Log = new ConsoleLog(),
        BackoffInterval = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(kafkaBackoff)),
        MinimumBytes = 0,
        MaxWaitTimeForMinimumBytes = new TimeSpan(0, 0, 0, 0, 10)
      }, positions);

      semaphore = new AutoResetEvent(false);

      var consumingThread = new Thread(ConsumerWorker);
      consumingThread.Start();
    }

    private void ConsumerWorker()
    {
      foreach (var message in consumer.Consume())
      {
        lock (lockObject)
        {
          internalStack.Enqueue(message);
          semaphore.Set();
        }
      }
    }

    private object Deserialize(byte[] data, Type type)
    {
      try
      {
        using (var stream = new MemoryStream(data))
        using (var reader = new StreamReader(stream))
          return JsonSerializer.Create().Deserialize(reader, type);
      }
      catch
      {
        var text = data.ToUtf8String();
        Log.WarnFormat("KafkaQueue: Can not deserialize {0}", text);
      }
      return null;
    }

    public virtual T GetNextItem(out long offset)
    {
      if (internalStack.Count == 0)
        semaphore.WaitOne();

      lock (lockObject)
      {
        if (internalStack.Count > 0)
        {
          var message = internalStack.Dequeue();
          offset = message.Meta.Offset;
          var text = message.Value.ToUtf8String();
          Log.DebugFormat("KafkaQueue: Processing message {0}, stack size is {1}", text, internalStack.Count);
          if (internalStack.Count == 0)
            semaphore.Reset();
          return (T) Deserialize(message.Value, typeof (T));
        }
        offset = 0;
        return default(T);
      }
    }
  }
}