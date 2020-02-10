using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Device.AcceptanceTests.Utils.Config
{
  class DeviceFirmwareSqlQueries
  {
    public static string MysqlDB = DeviceServiceConfig.MySqlDBName;
    public static string DeviceFirmwareCellularRadioFirmware= "SELECT count(1) FROM `" + MysqlDB + "`.Device WHERE DeviceUID=unhex('{0}') AND CELLULARFIRMWAREPARTNUMBER='{1}'";
    public static string DeviceFirmwareNetworkFirmware = "SELECT count(1) FROM `" + MysqlDB + "`.Device WHERE DeviceUID=unhex('{0}') AND NETWORKFIRMWAREPARTNUMBER='{1}'";
    public static string DeviceSatelliteRadioNetworkFirmware = "SELECT count(1) FROM `" + MysqlDB + "`.Device WHERE DeviceUID=unhex('{0}') AND SATELLITEFIRMWAREPARTNUMBER='{1}'";
  }
}
