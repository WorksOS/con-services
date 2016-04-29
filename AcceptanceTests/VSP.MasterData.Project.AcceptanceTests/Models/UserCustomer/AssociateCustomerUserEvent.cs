using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSP.MasterData.Project.AcceptanceTests.Models.UserCustomer
{
    public class AssociateCustomerUserEvent
    {
        public Guid CustomerUID { get; set; }
        public Guid UserUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
