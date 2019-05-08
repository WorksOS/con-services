using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
	public class FavoriteGeofenceEvent : IGeofenceEvent
	{
		public Guid GeofenceUID { get; set; }
		public Guid UserUID { get; set; }
    public Guid CustomerUID { get; set; }
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }
	}
}
