using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceSettings.Enums;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
	public interface IFuelUpdateRequestEventGenerator
    {
        IMTSOutMessageEvent GetFuelMessageForMTS(DeviceDetails deviceDetails, FuelRequestType fuelRequest);
        IPLOutMessageEvent GetFuelMessageForPLOut(DeviceDetails deviceDetails);
    }
}
