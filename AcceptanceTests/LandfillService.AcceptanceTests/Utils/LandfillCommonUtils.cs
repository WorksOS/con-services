using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Utils
{
    public class LandfillCommonUtils
    {
        public readonly static Random Random = new Random((int)(DateTime.Now.Ticks % 1000000));
    }
}
