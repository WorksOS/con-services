using KafkaConsumer;
using KafkaConsumer.JsonConverters;
using Newtonsoft.Json;
using VSS.Project.Service.Utils.JsonConverters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using KafkaConsumer.Interfaces;

namespace KafkaConsumer
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

      return null;
    }
  }
}