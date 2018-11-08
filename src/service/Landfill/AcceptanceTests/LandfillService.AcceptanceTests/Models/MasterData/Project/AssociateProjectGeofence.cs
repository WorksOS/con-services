using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models
{
    public class AssociateProjectGeofence
    {
        public DateTime ActionUTC { get; set; }
        public Guid GeofenceUID { get; set; }
        public Guid ProjectUID { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
