using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	public class UpdateDeviceRequestStatusBuilder : IUpdateDeviceRequestStatusBuilder
    {
        private ILocationUpdateRequestEventGenerator _locationUpdateRequestEventGenerator;
        private IFuelUpdateRequestEventGenerator _fuelUpdateRequestEventGenerator;
        private IECMInfoUpdateRequestEventGenerator _ecmInfoUpdateRequestEventGenerator;
        private ITPMSPingUpdateRequestEventGenerator _tpmsPingUpdateRequestEventGenerator;
        private IPTOHoursVia1939UpdateRequestEventGenerator _ptoHoursUpdateRequestEventGenerator;
        private IBatteryVoltageVia1939UpdateRequestEventGenerator _batteryVolatageVia1939UpdateRequestEventGenerator;
        private IEventDiagonsticUpdateRequestEventGenerator _eventDiagonsticUpdateRequestEventGenerator;

        const string DEVICE_CAPABILITY_SUPPORT_GATEWAY_FOR_FUEL_MTS = @"GatewayRequestMessage";
        const string DEVICE_CAPABILITY_SUPPORT_BUS_REQUEST_FOR_ECM_FUEL_TPMSPARTIAL_MTS = @"BusRequestMessage";
        const string DEVICE_CAPABILITY_SUPPORT_FOR_TPMSPARTIAL_MTS = @"TMSPing";
        const string DEVICE_CAPABILITY_SUPPORT_FOR_PTO_MTS = @"PTOHoursViaJ1939";
        const string DEVICE_CAPABILITY_SUPPORT_FOR_BATTERY_MTS = @"BatteryVoltageViaJ1939";
        const string DEVICE_CAPABILITY_SUPPORT_FOR_FUEL_PL = @"ReportFuel";

        public UpdateDeviceRequestStatusBuilder(ILocationUpdateRequestEventGenerator locationUpdateRequestEventGenerator, 
                                                IFuelUpdateRequestEventGenerator fuelUpdateRequestEventGenerator,
                                                IECMInfoUpdateRequestEventGenerator ecmInfoUpdateRequestEventGenerator,
                                                ITPMSPingUpdateRequestEventGenerator tpmsPingUpdateRequestEventGenerator,
                                                IPTOHoursVia1939UpdateRequestEventGenerator ptoHoursUpdateRequestEventGenerator,
                                                IBatteryVoltageVia1939UpdateRequestEventGenerator batteryVolatageVia1939UpdateRequestEventGenerator,
                                                IEventDiagonsticUpdateRequestEventGenerator eventDiagonsticUpdateRequestEventGenerator
            )
        {
            _locationUpdateRequestEventGenerator = locationUpdateRequestEventGenerator;
            _fuelUpdateRequestEventGenerator = fuelUpdateRequestEventGenerator;
            _ecmInfoUpdateRequestEventGenerator = ecmInfoUpdateRequestEventGenerator;
            _tpmsPingUpdateRequestEventGenerator = tpmsPingUpdateRequestEventGenerator;
            _ptoHoursUpdateRequestEventGenerator = ptoHoursUpdateRequestEventGenerator;
            _batteryVolatageVia1939UpdateRequestEventGenerator = batteryVolatageVia1939UpdateRequestEventGenerator;
            _eventDiagonsticUpdateRequestEventGenerator = eventDiagonsticUpdateRequestEventGenerator;
        }

        public IEnumerable<IOutMessageEvent> BuildA5N2DeviceStatusUpdateRequestMessage(DeviceDetails deviceDetails, IDictionary<string, string> _deviceCapability)
        {
            /*
             * IF A5N2, By default only Location Messages Are Supported
             */
            IList<IOutMessageEvent> _dataOutMessageEvent = new List<IOutMessageEvent>();
            _dataOutMessageEvent.Add(_locationUpdateRequestEventGenerator.GetLocationMessageForA5N2(deviceDetails));
            return _dataOutMessageEvent;
        }

        public IEnumerable<IMTSOutMessageEvent> BuildMTSDeviceStatusUpdateRequestMessage(DeviceDetails deviceDetails, IDictionary<string, string> _deviceCapability)
        {
            /*
             * Mapping
             * Hours & Location / Location Status message :- By Default
             * Fuel                                       :- GatewayRequestMessage (SendGateWayRequestMessage) Or  BusRequestMessage (SendVehicleRequestMessage)
             * ECMINfo                                    :- BusRequestMessage
             * TPMS		                                  :- BusRequestMessage And TMSPing
             * PTO Hours via J1939                        :- PTOHoursViaJ1939
             * Battery Voltage Via J1939                  :- BatteryVoltageViaJ1939
             */
            IList<IMTSOutMessageEvent> _mtsOutMessageEvent = new List<IMTSOutMessageEvent>();
            _mtsOutMessageEvent.Add(_locationUpdateRequestEventGenerator.GetLocationMessageForMTS(deviceDetails));
            if (isFuelSupportedForThisMTSDevice(_deviceCapability))
                _mtsOutMessageEvent.Add(_fuelUpdateRequestEventGenerator.GetFuelMessageForMTS(deviceDetails, GetRequestType(_deviceCapability)));
            if (isECMSupported(_deviceCapability)) {
                _mtsOutMessageEvent.Add(_ecmInfoUpdateRequestEventGenerator.GetECMRequestMessageForMTS(deviceDetails));
                _mtsOutMessageEvent.Add(_ecmInfoUpdateRequestEventGenerator.GetDevicePersonalityRequest(deviceDetails));
            }
            if (isTPMSupported(_deviceCapability))
                _mtsOutMessageEvent.Add(_tpmsPingUpdateRequestEventGenerator.GetTPMSRequestMessageForMTS(deviceDetails));
            if (isPTOHoursViaJ1939Supported(_deviceCapability))
                _mtsOutMessageEvent.Add(_ptoHoursUpdateRequestEventGenerator.GetPTOHoursviaJ1939(deviceDetails));
            if (isBatteryVoltageViaJ1939Supported(_deviceCapability))
                _mtsOutMessageEvent.Add(_batteryVolatageVia1939UpdateRequestEventGenerator.GetBatteryVoltageVia1939(deviceDetails));
            return _mtsOutMessageEvent;
        }

        public IEnumerable<IPLOutMessageEvent> BuildPLDeviceStatusUpdateRequestMessage(DeviceDetails deviceDetails, IDictionary<string, string> _deviceCapability)
        {
            /*
             * IF PL Device, By default only LocationHours and Event Diagonstic Messages Are Supported
             */
            IList<IPLOutMessageEvent> _plOutMessageEvent = new List<IPLOutMessageEvent>();
            _plOutMessageEvent.Add(_locationUpdateRequestEventGenerator.GetLocationMessageForPLDevices(deviceDetails));
            _plOutMessageEvent.Add(_eventDiagonsticUpdateRequestEventGenerator.GetEventDiagonsticUpdateRequestEventGenerator(deviceDetails));
            if (isFuelSupportedForThisPLDevices(_deviceCapability))
                _plOutMessageEvent.Add(_fuelUpdateRequestEventGenerator.GetFuelMessageForPLOut(deviceDetails));
            return _plOutMessageEvent;
        }

        #region "Private Methods"
        private FuelRequestType GetRequestType(IDictionary<string, string> _deviceCapability)
        {
            return _deviceCapability.ContainsKey(DEVICE_CAPABILITY_SUPPORT_GATEWAY_FOR_FUEL_MTS) && Convert.ToBoolean(_deviceCapability[DEVICE_CAPABILITY_SUPPORT_GATEWAY_FOR_FUEL_MTS]) ?
                FuelRequestType.GatewayRequest : FuelRequestType.BusRequest;
        }
        private bool isFuelSupportedForThisMTSDevice(IDictionary<string, string> _deviceCapability)
        {
            return _deviceCapability.ContainsKey(DEVICE_CAPABILITY_SUPPORT_GATEWAY_FOR_FUEL_MTS) && Convert.ToBoolean(_deviceCapability[DEVICE_CAPABILITY_SUPPORT_GATEWAY_FOR_FUEL_MTS])
                || _deviceCapability.ContainsKey(DEVICE_CAPABILITY_SUPPORT_BUS_REQUEST_FOR_ECM_FUEL_TPMSPARTIAL_MTS) && Convert.ToBoolean(_deviceCapability[DEVICE_CAPABILITY_SUPPORT_BUS_REQUEST_FOR_ECM_FUEL_TPMSPARTIAL_MTS]);
        }

        private bool isFuelSupportedForThisPLDevices(IDictionary<string, string> _deviceCapability)
        {
            return _deviceCapability.ContainsKey(DEVICE_CAPABILITY_SUPPORT_FOR_FUEL_PL) && Convert.ToBoolean(_deviceCapability[DEVICE_CAPABILITY_SUPPORT_FOR_FUEL_PL]);
        }

        private bool isECMSupported(IDictionary<string, string> _deviceCapability)
        {
            return _deviceCapability.ContainsKey(DEVICE_CAPABILITY_SUPPORT_BUS_REQUEST_FOR_ECM_FUEL_TPMSPARTIAL_MTS) && Convert.ToBoolean(_deviceCapability[DEVICE_CAPABILITY_SUPPORT_BUS_REQUEST_FOR_ECM_FUEL_TPMSPARTIAL_MTS]);
        }
        private bool isTPMSupported(IDictionary<string, string> _deviceCapability)
        {
            return _deviceCapability.ContainsKey(DEVICE_CAPABILITY_SUPPORT_BUS_REQUEST_FOR_ECM_FUEL_TPMSPARTIAL_MTS) && Convert.ToBoolean(_deviceCapability[DEVICE_CAPABILITY_SUPPORT_BUS_REQUEST_FOR_ECM_FUEL_TPMSPARTIAL_MTS])
                && _deviceCapability.ContainsKey(DEVICE_CAPABILITY_SUPPORT_BUS_REQUEST_FOR_ECM_FUEL_TPMSPARTIAL_MTS) && Convert.ToBoolean(_deviceCapability[DEVICE_CAPABILITY_SUPPORT_BUS_REQUEST_FOR_ECM_FUEL_TPMSPARTIAL_MTS]);
        }
        private bool isPTOHoursViaJ1939Supported(IDictionary<string, string> _deviceCapability)
        {
            return _deviceCapability.ContainsKey(DEVICE_CAPABILITY_SUPPORT_FOR_PTO_MTS) && Convert.ToBoolean(_deviceCapability[DEVICE_CAPABILITY_SUPPORT_FOR_PTO_MTS]);
        }
        private bool isBatteryVoltageViaJ1939Supported(IDictionary<string, string> _deviceCapability)
        {
            return _deviceCapability.ContainsKey(DEVICE_CAPABILITY_SUPPORT_FOR_PTO_MTS) && Convert.ToBoolean(_deviceCapability[DEVICE_CAPABILITY_SUPPORT_FOR_PTO_MTS]);
        }
        #endregion
    }
}
