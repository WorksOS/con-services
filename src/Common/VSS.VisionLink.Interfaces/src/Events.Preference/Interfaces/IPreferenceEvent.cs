using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Interfaces.Events.Preference.Interfaces
{
	public interface IPreferenceEvent
	{
		DateTime ActionUTC { get; set; }
	}
}
