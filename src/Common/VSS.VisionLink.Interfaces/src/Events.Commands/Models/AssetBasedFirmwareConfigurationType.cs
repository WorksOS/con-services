using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public enum AssetBasedFirmwareConfigurationType : byte
	{
		PL420VocationalTrucks = 1,
		PL421BCP = 2,
		PL420EPD = 3,
		PL420AfterMarket1 = 4,
		PL420AfterMarket2 = 5,
		PL421China = 6,
		PL421Forestry = 7,
		PL421Paving = 8,
		PL4XXRFID = 9,
		PL4XXJ1939ScanMode = 254
	}

}
