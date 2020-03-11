using System;

namespace Infrastructure.Common.DeviceMessageConstructor.Models
{
	public class MTSServerSideRunTimeCalibration
	{
		public string DeviceSerialNumber { get; set; }
		public string DeviceType { get; set; }
		public double ProposedRunTimeHours { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime ActionUtc { get; set; }
	}
}
