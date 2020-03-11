using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.VolumePerCycle
{
    public class POSTVolumePerCycleResponse
    {
        public Guid assetConfigUID { get; set; }
        public int targetValue { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public Guid assetuid { get; set; }
    }
}
