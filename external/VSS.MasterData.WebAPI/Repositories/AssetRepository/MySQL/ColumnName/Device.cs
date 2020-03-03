using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.AssetRepository.MySql.ColumnName
{
    public static class Device
    {
        internal static class ColumnName
        {
            public const string DEVICE_UID = "DeviceUID";
            public const string DEVICE_SERIAL_NUMBER = "SerialNumber";
            public const string DEVICE_TYPE = "fk_DeviceTypeID";
            public const string DEVICE_STATE = "fk_DeviceStatusID";
        }
    } 
}

