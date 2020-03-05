using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
   public class DeviceDetails
   {
		public Guid AssetUid { get; set; }
		public Guid DeviceUid { get; set; }
		public string DeviceType { get; set; }
		public string SerialNumber { get; set; }
		public DateTime EventUtc { get; set; }
		public Guid MessageUid { get; set; }
		public ParamGroup Group { get; set; }
		public Guid UserUid { get; set; }
	}
}
