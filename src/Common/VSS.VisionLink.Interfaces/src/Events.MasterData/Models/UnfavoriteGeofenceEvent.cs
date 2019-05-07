using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
	public class UnfavoriteGeofenceEvent : IGeofenceEvent
	{
		public Guid GeofenceUID { get; set; }
		public Guid UserUID { get; set; }
    public Guid CustomerUID { get; set; }
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }
	}
}