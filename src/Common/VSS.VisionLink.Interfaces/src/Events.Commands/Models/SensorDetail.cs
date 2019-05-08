namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public class SensorDetail
	{
		public bool Enabled { get; set; }
		public bool IgnReqired { get; set; }
		public double HystHalfSec { get; set; }
		public bool HasPosPolarity { get; set; }
	}
}