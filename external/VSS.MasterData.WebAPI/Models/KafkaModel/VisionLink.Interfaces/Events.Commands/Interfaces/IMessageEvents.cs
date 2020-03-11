using System;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.Interfaces
{
	public interface IOutMessageEvent
	{
	  EventContext Context { get; set; }
	}

	public interface IMTSOutMessageEvent
	{
		EventContext Context { get; set; }
	}

	public interface IPLOutMessageEvent
	{
		EventContext Context { get; set; }
	}
	
}