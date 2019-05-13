using System.Collections.Generic;
using System.Xml.Linq;
using VSS.VisionLink.Interfaces.Events.Commands.Helpers;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.MTS
{

	/// <summary>
	/// Site Purge
	/// </summary>
	public class SendPurgeSitesEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
	}
	
	/// <summary>
	/// Specific site deassign
	/// </summary>
	public class DeassignSiteEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public long SiteId { get; set; }		
	}
	
	/// <summary>
	/// Assign site for Crosscheck
	/// </summary>
	public class SiteDispatchEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public long SiteId { get; set; }
		public long MaxLat { get; set; }
		public long MaxLon { get; set; }
		public long MinLat { get; set; }
		public long MinLon { get; set; }
		public string SiteName { get; set; }
	}

	/// <summary>
	/// For device types that support SiteDispatch
	/// </summary>
	public class ConfigureSitePolygonEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public long SiteId { get; set; }
		public string PolygonWKT { get; set; }
	}

	
}