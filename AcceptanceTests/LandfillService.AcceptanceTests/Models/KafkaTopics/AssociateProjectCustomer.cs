using System;
using LandfillService.AcceptanceTests.Interfaces;

namespace LandfillService.AcceptanceTests.Models.KafkaTopics
{
    public class AssociateProjectCustomer : IProjectEvent
    {
        public Guid ProjectUID { get; set; }
        public Guid CustomerUID { get; set; }
        public RelationType RelationType { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}