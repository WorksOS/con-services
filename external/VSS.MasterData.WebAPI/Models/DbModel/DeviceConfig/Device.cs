using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.DeviceConfig
{
   public class DeviceTypeFamily
   {
		public string TypeName { get; set; }
		public int DeviceTypeId { get; set; }
		public string FamilyName { get; set; }
   }
    public class DeviceData
    {
        public string AssetUid { get; set; }
        public string DeviceUid { get; set; }
        public string SerialNumber { get; set; }
        public string DeviceType { get; set; }
    }

    public class DeviceConfigMsg
    {
        public Guid AssetUid { get; set; }
        public Guid DeviceUid { get; set; }
        public string DeviceType { get; set; }
        public string SerialNumber { get; set; }
        public DateTime EventUtc { get; set; }
        public Guid MessageUid { get; set; }
        public string MessageContent { get; set; }
        public Guid UserUid { get; set; }
    }

    public class DeviceFeatures
    {
        public string DeviceFeatureName { get; set; }
        public bool isSupported { get; set; }
    }
}
