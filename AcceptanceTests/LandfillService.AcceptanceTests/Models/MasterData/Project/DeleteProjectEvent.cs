using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandfillService.AcceptanceTests.Models.MasterData.Interfaces;

namespace LandfillService.AcceptanceTests.Models.MasterData.Project
{
    public class DeleteProjectEvent : IProjectEvent
    {
        public Guid ProjectUID { get; set; }
        public bool DeletePermanently { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
