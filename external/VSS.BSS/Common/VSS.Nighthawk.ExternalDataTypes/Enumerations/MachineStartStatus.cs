namespace VSS.Nighthawk.ExternalDataTypes.Enumerations
{
	public enum MachineStartStatus
	{
    //NormalOperation = 0x00,
    //Derated = 0x01,
    //Disabled = 0x02

    NotConfigured = -0x02,
    NoPending = -0x01,
    NormalOperation = 0x00,
    Derated = 0x01,
    Disabled = 0x02,
    NormalOperationPending = 0x10,
    DeratedPending = 0x11,
    DisabledPending = 0x12
	}
}
