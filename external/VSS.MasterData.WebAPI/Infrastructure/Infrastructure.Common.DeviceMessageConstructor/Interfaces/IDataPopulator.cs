using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Common.DeviceMessageConstructor.Interfaces
{
	public interface IDataPopulator
   {
      T ConstructPlEvent<T>(DeviceDetails deviceMessage) where T : IPLOutMessageEvent, new();
      T ConstructMtsEvent<T>(DeviceDetails deviceMessage) where T : IMTSOutMessageEvent, new();
      T ConstructDataOutEvent<T>(DeviceDetails deviceMessage) where T : IOutMessageEvent, new();
      T GetRequestModel<T>(DeviceConfigRequestBase requestBase) where T : DeviceConfigRequestBase;
      T GetEventEnumValue<T>(string sourceEnumValue) where T : struct;
   }
}
