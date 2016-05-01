using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSP.MasterData.Project.AcceptanceTests.Models.Project
{
    public class AssociateProjectCustomer
    {
        public Guid ProjectUID { get; set; }
        public Guid CustomerUID { get; set; }
        public RelationType RelationType { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
