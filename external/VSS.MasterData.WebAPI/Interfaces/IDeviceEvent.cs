using System;

namespace Interfaces
{
	/// <summary>
	/// Device Event Interface
	/// </summary>
	public interface IDeviceEvent
	{
		/// <summary>
		/// Device event action UTC time
		/// </summary>
		DateTime ActionUTC { get; set; }
		/// <summary>
		/// Device event received UTC time
		/// </summary>
		DateTime ReceivedUTC { get; set; }
	}
}