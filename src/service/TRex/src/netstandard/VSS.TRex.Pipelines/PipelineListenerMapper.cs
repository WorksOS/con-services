using System;
using System.Collections.Concurrent;
using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SubGrids.GridFabric.Listeners;

namespace VSS.TRex.Pipelines
{
  /// <summary>
  /// Defines a mapping between task IDs and the ITRexTask instances representing the task in a pipeline
  /// </summary>
  public class PipelineListenerMapper : IPipelineListenerMapper
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridListener>();

    private readonly ConcurrentDictionary<Guid, IMessageListener<ISerialisedByteArrayWrapper>> _listenerMap;

    public PipelineListenerMapper()
    {
      _listenerMap = new ConcurrentDictionary<Guid, IMessageListener<ISerialisedByteArrayWrapper>>();
    }

    public void Add(Guid requestDescriptor, IMessageListener<ISerialisedByteArrayWrapper> listener)
    {
      if (requestDescriptor == Guid.Empty)
      {
        throw new ArgumentException("requestDescriptor cannot be empty");
      }

      if (!_listenerMap.TryAdd(requestDescriptor, listener))
      {
        Log.LogError($"Request descriptor ID {requestDescriptor} already has a task mapping");
      }
    }

    public void Remove(Guid requestDescriptor, IMessageListener<ISerialisedByteArrayWrapper> listener)
    {
      if (!_listenerMap.TryRemove(requestDescriptor, out var l))
      {
        Log.LogError($"Request descriptor {requestDescriptor} listener mapping not present on Remove()");
      }

      if (l != listener)
      {
        throw new ArgumentException("Supplied listener is not the same as the one provided for the request descriptor");
      }
    }

    public IMessageListener<ISerialisedByteArrayWrapper> Find(Guid pipelineId)
    {
      if (!_listenerMap.TryGetValue(pipelineId, out var listener))
      {
        Log.LogError($"Pipeline ID {pipelineId} listener mapping not present on Find()");
      }

      return listener;
    }
  }
}
