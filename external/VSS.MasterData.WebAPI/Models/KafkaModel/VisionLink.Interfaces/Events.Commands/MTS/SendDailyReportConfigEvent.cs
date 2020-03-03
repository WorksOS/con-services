using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.MTS
{
	public class SendDailyReportConfigEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public bool Enabled { get; set; }
		public byte DailyReportTimeHour { get; set; }
		public byte DailyReportTimeMinute { get; set; }
		public string TimezoneName { get; set; }
	}
}