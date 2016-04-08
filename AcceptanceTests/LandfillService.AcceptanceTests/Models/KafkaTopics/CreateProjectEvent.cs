using System;
using LandfillService.AcceptanceTests.Interfaces;

namespace LandfillService.AcceptanceTests.Models.KafkaTopics
{
    public class CreateProjectEvent : IProjectEvent
    {
        public DateTime ProjectEndDate { get; set; }
        public DateTime ProjectStartDate { get; set; }
        public string ProjectTimezone { get; set; }
        public string ProjectName { get; set; }
        public ProjectType ProjectType { get; set; }
        public string ProjectBoundary { get; set; }


        public Guid ProjectUID { get; set; }
        public int ProjectID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}


