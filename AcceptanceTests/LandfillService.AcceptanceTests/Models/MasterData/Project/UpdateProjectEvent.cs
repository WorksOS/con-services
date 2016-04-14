using System;

namespace LandfillService.AcceptanceTests.Models
{
    public class UpdateProjectEvent
    {
        public DateTime ProjectEndDate { get; set; }
        public string ProjectTimezone { get; set; }
        public string ProjectName { get; set; }
        public ProjectType ProjectType { get; set; }

        public Guid ProjectUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
