using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using Infrastructure.Common.DeviceMessageConstructor.Settings;
using System;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace Infrastructure.Common.DeviceMessageConstructor.Implementation
{
	public class DataPopulator : IDataPopulator
    {
        //private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public T ConstructPlEvent<T>(DeviceDetails deviceMessage) where T : IPLOutMessageEvent, new()
        {
            var messageEvent = new T();
            messageEvent.Context = new EventContext
            {
                AssetUid = deviceMessage.AssetUid.ToString(),
                DeviceId = deviceMessage.SerialNumber,
                DeviceType = deviceMessage.DeviceType,
                DeviceUid = deviceMessage.DeviceUid.ToString(),
                EventUtc = deviceMessage.EventUtc
            };
            return messageEvent;
        }

        public T ConstructMtsEvent<T>(DeviceDetails deviceMessage) where T : IMTSOutMessageEvent, new()
        {
            var messageEvent = new T();
            messageEvent.Context = new EventContext
            {
                AssetUid = deviceMessage.AssetUid.ToString(),
                DeviceId = deviceMessage.SerialNumber,
                DeviceType = deviceMessage.DeviceType,
                DeviceUid = deviceMessage.DeviceUid.ToString(),
                EventUtc = deviceMessage.EventUtc
            };
            return messageEvent;
        }

        public T ConstructDataOutEvent<T>(DeviceDetails deviceMessage) where T : IOutMessageEvent, new()
        {
            var messageEvent = new T();
            messageEvent.Context = new EventContext
            {
                AssetUid = deviceMessage.AssetUid.ToString(),
                DeviceId = deviceMessage.SerialNumber,
                DeviceType = deviceMessage.DeviceType,
                DeviceUid = deviceMessage.DeviceUid.ToString(),
                EventUtc = deviceMessage.EventUtc
            };
            return messageEvent;
        }

        public T GetRequestModel<T>(DeviceConfigRequestBase requestBase) where T : DeviceConfigRequestBase
        {
            if (requestBase is T)
                return requestBase as T;
            throw new InvalidCastException("Unable to Cast requestbase to given Type " + requestBase.GetType().Name);
      }

      public T GetEventEnumValue<T>(string sourceEnumValue) where T : struct
      {
         var typeName = typeof (T).Name;
         string outEnumValue = String.Empty;
         if (EnumMapper.Container.ContainsKey(typeName) && EnumMapper.Container[typeName].ContainsKey(sourceEnumValue))
            outEnumValue = EnumMapper.Container[typeName][sourceEnumValue];
         else
            outEnumValue = sourceEnumValue;
         foreach (T item in Enum.GetValues(typeof (T)))
         {
            if (item.ToString().ToLower().Equals(outEnumValue.ToLower()))
               return item;
         }
         throw new ArgumentException("Invalid Enum Mapping done " + sourceEnumValue);
        }
    }
}
