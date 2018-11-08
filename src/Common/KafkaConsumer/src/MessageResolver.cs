using Newtonsoft.Json;
using VSS.KafkaConsumer.Interfaces;
using VSS.KafkaConsumer.JsonConverters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.KafkaConsumer
{
  public class MessageResolver : IMessageTypeResolver
  {
    public JsonConverter GetConverter<T>()
    {
      if (typeof(T) == typeof(IAssetEvent))
        return new AssetEventConverter();
      if (typeof(T) == typeof(ICustomerEvent))
        return new CustomerEventConverter();
      if (typeof(T) == typeof(IDeviceEvent))
        return new DeviceEventConverter();
      if (typeof(T) == typeof(IGeofenceEvent))
        return new GeofenceEventConverter();
      if (typeof(T) == typeof(IProjectEvent))
        return new ProjectEventConverter();
      if (typeof(T) == typeof(ISubscriptionEvent))
        return new SubscriptionEventConverter();
      if (typeof(T) == typeof(IFilterEvent))
        return new FilterEventConverter();

      return null;
    }
  }
}