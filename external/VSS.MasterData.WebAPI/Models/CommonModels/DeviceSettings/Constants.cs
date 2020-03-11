using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class Constants
    {
        public const string PL_REPORTINGSCHEDULE_GROUP = "ReportingSchedule";
        public const string PL_METERS_GROUP = "Meters";
        public const string PL_SWITCHES_GROUP = "Switches";
        public const string PL_FAULTCODEREPORTING_GROUP = "FaultCodeReporting";
        
        //Please make sure the value does not have any space after each comma. 
        public const string PING_ENABLED_DEVICE_TYPE_FAMILIES = "PL,MTS,DATAOUT";
    }
}
