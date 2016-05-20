using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models
{
    public class CreateGeofenceEvent
    {
        public DateTime ActionUTC { get; set; }
        public Guid CustomerUID { get; set; }
        public string Description { get; set; }
        public int FillColor { get; set; }
        public string GeofenceName { get; set; }
        public string GeofenceType { get; set; }
        public Guid GeofenceUID { get; set; }
        public string GeometryWKT { get; set; }
        public bool IsTransparent { get; set; }
        public DateTime ReceivedUTC { get; set; }
        public Guid UserUID { get; set; }
    }
}
