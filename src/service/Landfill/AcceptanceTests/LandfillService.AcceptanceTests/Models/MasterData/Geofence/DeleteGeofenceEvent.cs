using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models
{
    public class DeleteGeofenceEvent
    {
        public DateTime ActionUTC { get; set; }
        public Guid GeofenceUID { get; set; }
        public DateTime ReceivedUTC { get; set; }
        public Guid UserUID { get; set; }
    }
}
