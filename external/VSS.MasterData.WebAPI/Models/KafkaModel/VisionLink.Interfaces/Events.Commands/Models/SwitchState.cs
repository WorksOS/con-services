using System.Xml.Serialization;

namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public enum SwitchState
	{
		NotInstalled = 0x00,
		NotConfigured = 0x01,
		NormallyOpen = 0x02,
		NormallyClosed = 0x04
	}
}