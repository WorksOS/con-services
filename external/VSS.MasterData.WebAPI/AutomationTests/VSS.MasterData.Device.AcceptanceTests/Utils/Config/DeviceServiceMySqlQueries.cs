using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Device.AcceptanceTests.Utils.Config
{
    public abstract class DeviceServiceMySqlQueries
    {
        public const string CreateDeviceDetailsByDeviceUID = "SELECT COUNT(1) from Device Where DeviceUID=unhex('{0}') AND SerialNumber='{1}' AND DeregisteredUTC='{2}' AND ModuleType='{3}' AND MainboardSoftwareVersion='{4}' AND FirmwarePartNumber='{5}' AND GatewayFirmwarePartNumber='{6}' AND DataLinkType='{7}' AND fk_DeviceStatusID='{8}' AND fk_DeviceTypeID='{9}' AND CellModemIMEI='{10}' AND DevicePartNumber='{11}' AND CellularFirmwarePartNUmber='{12}' AND NetworkFirmwarePartNUmber='{13}' AND SatelliteFirmwarePartNumber='{14}';";

        public const string UpdateDeviceDetailsByDeviceUID = "SELECT COUNT(1) from Device Where DeviceUID=unhex('{0}') AND SerialNumber='{1}' AND DeregisteredUTC='{2}' AND ModuleType='{3}' AND MainboardSoftwareVersion='{4}' AND FirmwarePartNumber='{5}' AND GatewayFirmwarePartNumber='{6}' AND DataLinkType='{7}' AND fk_DeviceStatusID='{8}' AND fk_DeviceTypeID='{9}';";

    }
}
