namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public enum TamperResistanceStatus
	{
		NoPending = -0x01,
		Off = 0x00,
		TamperResistanceLevel1 = 0x01,
		TamperResistanceLevel2 = 0x02,
		TamperResistanceLevel3 = 0x03
	}
}