namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public enum PLQueryCommandEnum
	{
		PositionReportQuery = 1,
		SMUReportQuery,
		StatusQuery,
		EventDiagnosticQuery,
		FuelReportQuery,
		ProductWatchQuery,
		HardwareSoftwarePartNumber,
		RequestBDTAvailableFeatures,
		FuelLevelQuery,
		DeviceIDQuery,
		J1939EventDiagnosticQuery,
		Deregistration,
		ClearEvents,
		ProductWatchActivateDeactivate,
		RegistrationRequest,
		R2RegistrationRequest,
		ForcedDeregistration,
		BillingEnable,
		BillingDisable,
		InitialUpgradeRequest,
		UpgradeRequest,
		InitialDowngradeRequest,
		DowngradeRequest,
	}
	public enum EventFrequency
	{
		Unknown = 0,
		Immediately = 1,
		Next = 2,
		Never = 3,
	}
	public enum SMUFuelReporting
	{
		Off = 0x00,
		Fuel = 0x01,
		SMU = 0x02,
		SMUFUEL = 0x03,
		PL321VIMSFuel = 0x40,
		PL321VIMSSMU = 0x2F,
	}
	public enum InputConfig
	{
		NotInstalled = 0x11,
		NotConfigured = 0x2C,
		NormallyOpen = 0x57,
		NormallyClosed = 0x58,
	}
}