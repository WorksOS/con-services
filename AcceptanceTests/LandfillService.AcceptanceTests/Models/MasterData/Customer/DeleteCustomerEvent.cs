using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandfillService.AcceptanceTests.Models.MasterData.Interfaces;

namespace LandfillService.AcceptanceTests.Models.MasterData.Customer
{
    public class DeleteCustomerEvent : ICustomerEvent
    {
        public Guid CustomerUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
