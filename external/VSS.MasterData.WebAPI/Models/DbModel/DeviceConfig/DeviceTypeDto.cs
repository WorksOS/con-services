using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace DbModel.DeviceConfig
{
    public class DeviceTypeDto
    {
        private Guid _deviceTypeUID;
        public Guid DeviceTypeUID
        {
            get { return _deviceTypeUID; }
            set { this._deviceTypeUID = value; }
        }
        public string DeviceTypeUIDString
        {
            get { return _deviceTypeUID.ToStringWithoutHyphens(); }
            set { this._deviceTypeUID = Guid.Parse(value); }
        }
        public string DeviceTypeID { get; set; }
        public string TypeName { get; set; }
        public DateTime InsertUTC { get; set; }
        public DateTime UpdateUTC { get; set; }
    }
}
