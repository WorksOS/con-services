using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSP.MasterData.Project.AcceptanceTests.Utils
{
    public class CommonUtils
    {
        public readonly static Random Random = new Random((int)(DateTime.Now.Ticks % 1000000));
    }
}
