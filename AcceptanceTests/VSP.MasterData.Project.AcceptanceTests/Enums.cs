using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSP.MasterData.Project.AcceptanceTests
{
    public enum ProjectType
    {
        Standard = 0,
        LandFill = 1,
        ProjectMonitoring = 2
    }

    public enum CustomerType
    {
        Customer = 0,
        Dealer = 1,
        Operations = 2,
        Corporate = 3
    }

    public enum RelationType
    {
        Owner = 0,
        Customer = 1,
        Dealer = 2,
        Operations = 3,
        Corporate = 4,
        SharedOwner = 5
    }
}
