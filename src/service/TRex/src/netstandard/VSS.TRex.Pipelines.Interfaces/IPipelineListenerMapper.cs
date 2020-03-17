using System;
using Apache.Ignite.Core.Messaging;
using VSS.TRex.GridFabric;

namespace VSS.TRex.Pipelines.Interfaces
{
  public interface IPipelineListenerMapper
  {
    void Add(Guid requestDescriptor, IMessageListener<ISerialisedByteArrayWrapper> listener);

    void Remove(Guid requestDescriptor, IMessageListener<ISerialisedByteArrayWrapper> listener);

    IMessageListener<ISerialisedByteArrayWrapper> Find(Guid requestDescriptor);
  }
}
