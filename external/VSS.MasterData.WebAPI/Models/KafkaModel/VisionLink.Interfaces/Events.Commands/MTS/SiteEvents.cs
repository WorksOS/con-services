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
		public double MaxLat { get; set; }
		public double MaxLon { get; set; }
		public double MinLat { get; set; }
		public double MinLon { get; set; }
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