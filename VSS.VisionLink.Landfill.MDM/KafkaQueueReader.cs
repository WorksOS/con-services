using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.Unity;
using VSS.VisionLink.Landfill.Common.Interfaces;
using VSS.VisionLink.Landfill.Common.Models;
using VSS.VisionLink.Landfill.Common.Utilities;

namespace VSS.VisionLink.Landfill.DataFeed
{
  /// <summary>
  ///   Class used to read kafka queue. Contains tasks to this asynchronously
  /// </summary>
  public class KafkaQueueReader<T> where T : class
  {
    //Make kafka queues persistent
    // todo should this be combined with IMachineEvent Reader (KafkaQueReader.cs)?
    private static readonly Dictionary<string, Tuple<IKafkaQueue<T>, long, BookmarkTypeEnum>> EventQueues =
      new Dictionary<string, Tuple<IKafkaQueue<T>, long, BookmarkTypeEnum>>();

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly IKafkaQueue<T> kafkaQueue;
    private long queuedOffset;

    /// <summary>
    ///   Initializes a new instance of the <see cref="KafkaQueueReader{T}" /> class.
    /// </summary>
    /// <param name="dependencyContainer">The dependency resolution container.</param>
    /// <param name="kafkaTopic"></param>
    public KafkaQueueReader(UnityContainer dependencyContainer, string kafkaTopic)
    {
      ItemBookmarkType = BookmarkTypeEnum.None;

      if (EventQueues.ContainsKey(kafkaTopic))
      {
        kafkaQueue = EventQueues[kafkaTopic].Item1;
        QueuedOffset = EventQueues[kafkaTopic].Item2;
        ItemBookmarkType = EventQueues[kafkaTopic].Item3;
      }
      else
      {
        var queueEventType = TopicResolver.ResolveKafkaTopic(kafkaTopic);
        if (queueEventType == null)
        {
          throw new ArgumentException("Unknown kafka topic");
          //Note: Since this is in a task, it's an AggregateException that is received by the caller
        }
        var bookmarkType = GetBookmarkType(kafkaTopic);
        var lastBookmark = dependencyContainer.Resolve<IBookmarkRepository>().GetBookmark(bookmarkType);
        var offset = (int)(lastBookmark == null ? 0 : lastBookmark.Value);

        try
        {
          kafkaQueue = (IKafkaQueue<T>)dependencyContainer.Resolve(queueEventType, new ParameterOverrides
          {
            {"kafkaTopic", kafkaTopic},
            {"offset", offset}
          });
        }
        catch (Exception ex)
        {
          Log.ErrorFormat("Exception while creating Kafka queue {0} {1}", kafkaTopic, ex);
        }
        if (kafkaQueue == null)
        {
          throw new ArgumentException("Can not build kafkaQueue");
        }
        EventQueues.Add(kafkaTopic,
          new Tuple<IKafkaQueue<T>, long, BookmarkTypeEnum>(kafkaQueue, QueuedOffset, bookmarkType));
      }
    }

    /// <summary>
    ///   Gets the queued event in the reader.
    /// </summary>
    /// <value>
    ///   The queued event.
    /// </value>
    public T QueuedEvent { get; private set; }

    /// <summary>
    ///   Gets the offset of the queued event
    /// </summary>
    /// <value>
    ///   The offset of the queued event.
    /// </value>
    public long QueuedOffset
    {
      get { return queuedOffset; }
      private set { queuedOffset = value; }
    }

    public BookmarkTypeEnum ItemBookmarkType { get; private set; }

    private BookmarkTypeEnum GetBookmarkType(string kafkaTopic)
    {
      Log.DebugFormat("Resolving bookmark for IAssetEvent kafka topic {0}", kafkaTopic);
      var bookmarkType = BookmarkTypeEnum.None;
      if (kafkaTopic.Contains("CreateProjectEvent"))
      {
        bookmarkType = BookmarkTypeEnum.CreateProjectEvent;
      }
      else if (kafkaTopic.Contains("UpdateProjectEvent"))
      {
        bookmarkType = BookmarkTypeEnum.UpdateProjectEvent;
      }
      else if (kafkaTopic.Contains("DeleteProjectEvent"))
      {
        bookmarkType = BookmarkTypeEnum.DeleteProjectEvent;
      }
      else if (kafkaTopic.Contains("CreateSubscriptionEvent"))
      {
        bookmarkType = BookmarkTypeEnum.CreateSubscriptionEv;
      }
      else if (kafkaTopic.Contains("UpdateSubscriptionEvent"))
      {
        bookmarkType = BookmarkTypeEnum.UpdateSubscriptionEv;
      }
      Log.DebugFormat("Bookmark type {0}", bookmarkType);
      return bookmarkType;
    }

    public async Task<bool> ReadNextEvent()
    {
      Log.DebugFormat("Landfill MDM: about to read next message of type {0}", typeof(T));
      long offset = 0;
      var task = new Task<T>(() => kafkaQueue.GetNextItem(out offset));
      task.Start();
      var assetEvent = await task;
      if (assetEvent != null)
      {
        QueuedEvent = assetEvent;
        queuedOffset = offset;
        return true;
      }
      return false;
    }
  }
}
