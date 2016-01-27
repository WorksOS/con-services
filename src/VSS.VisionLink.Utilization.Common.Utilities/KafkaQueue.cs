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
using Newtonsoft.Json.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
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
        if (type.IsInterface)
        {
          var bytesAsString = data.ToUtf8String();
          if (type.Name.Contains("IProjectEvent"))
            return JsonConvert.DeserializeObject<IProjectEvent>(bytesAsString, new ProjectEventConverter());
        }
        else
        {
          using (var stream = new MemoryStream(data))
          using (var reader = new StreamReader(stream))
            return JsonSerializer.Create().Deserialize(reader, type);
        }
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

  public class ProjectEventConverter : JsonCreationConverter<IProjectEvent>
  {
    protected override IProjectEvent Create(Type objectType, JObject jObject)
    {
      if (jObject["CreateProjectEvent"] != null)
      {
        return jObject["CreateProjectEvent"].ToObject<CreateProjectEvent>();
      }
      if (jObject["DeleteProjectEvent"] != null)
      {
        return jObject["DeleteProjectEvent"].ToObject<DeleteProjectEvent>();
      }
      if (jObject["UpdateProjectEvent"] != null)
      {
        return jObject["UpdateProjectEvent"].ToObject<UpdateProjectEvent>();
      }
      if (jObject["RestoreProjectEvent"] != null)
      {
        return jObject["RestoreProjectEvent"].ToObject<RestoreProjectEvent>();
      }
      if (jObject["AssociateProjectCustomerEvent"] != null)
      {
        return jObject["AssociateProjectCustomerEvent"].ToObject<AssociateProjectCustomer>();
      }
      if (jObject["DissociateProjectCustomerEvent"] != null)
      {
        return jObject["DissociateProjectCustomerEvent"].ToObject<DissociateProjectCustomer>();
      }

      return null;
    }
  }

  public abstract class JsonCreationConverter<T> : JsonConverter
  {
    /// <summary>
    /// Create an instance of objectType, based properties in the JSON object
    /// </summary>
    /// <param name="objectType">type of object expected</param>
    /// <param name="jObject">
    /// contents of JSON object that will be deserialized
    /// </param>
    /// <returns></returns>
    protected abstract T Create(Type objectType, JObject jObject);

    public override bool CanConvert(Type objectType)
    {
      return typeof(T).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader,
                                    Type objectType,
                                     object existingValue,
                                     JsonSerializer serializer)
    {
      if (reader.TokenType == JsonToken.Null)
        return null;

      // Load JObject from stream
      JObject jObject = JObject.Load(reader);

      // Create and populate target object based on JObject
      T target = Create(objectType, jObject);

      return target;
    }

    public override void WriteJson(JsonWriter writer,
                                   object value,
                                   JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }

  }
}