using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Geofence.Processor.Interfaces
{
	public interface IConsumer
	{
		void Consume();
		void Dispose();
	}
}
