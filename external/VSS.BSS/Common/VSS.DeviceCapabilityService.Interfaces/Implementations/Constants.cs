namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations
{
  // These constants are contracts for controller action names
  // and are included here for reference by the client code
  public static class ActionConstants
  {
    public const string GetAssetIdConfigurationChangedEvent = "IAssetIdConfigurationChangedEvent";
    public const string GetDigitalSwitchConfigurationEvent = "IDigitalSwitchConfigurationEvent";
    public const string GetDisableMaintenanceModeEvent = "IDisableMaintenanceModeEvent";
    public const string GetDiscreteInputConfigurationEvent = "IDiscreteInputConfigurationEvent";
    public const string GetEnableMaintenanceModeEvent = "IEnableMaintenanceModeEvent";
    public const string GetFirstDailyReportStartTimeUtcChangedEvent = "IFirstDailyReportStartTimeUtcChangedEvent";
    public const string GetHourMeterModifiedEvent = "IHourMeterModifiedEvent";
    public const string GetLocationUpdateRequestEvent = "ILocationStatusUpdateRequestedEvent";
    public const string GetMovingCriteriaConfigurationChangedEvent = "IMovingCriteriaConfigurationChangedEvent";
    public const string GetOdometerModifiedEvent = "IOdometerModifiedEvent";
    public const string GetSiteDispatchedEvent = "ISiteDispatchedEvent";
    public const string GetSiteRemovedEvent = "ISiteRemovedEvent";
    public const string SetStartModeEvent = "ISetStartModeEvent";
    public const string GetStartModeEvent = "IGetStartModeEvent";
    public const string SetTamperLevelEvent = "ISetTamperLevelEvent";
    public const string GetTamperLevelEvent = "IGetTamperLevelEvent";
    public const string SetDailyReportFrequencyEvent = "ISetDailyReportFrequencyEvent";
    public const string EnableRapidReportingEvent = "IEnableRapidReportingEvent";
    public const string DisableRapidReportingEvent = "IDisableRapidReportingEvent";
    public const string ReportingFrequencyChangedEvent = "IReportingFrequencyChangedEvent";
  }

  public static class ControllerConstants
  {
    public const string AssetSettingsControllerRouteName = "AssetSettings";
    public const string DeviceConfigControllerRouteName = "DeviceConfig";
    public const string LocationUpdateRequestedControllerRouteName = "LocationUpdateRequested";
    public const string SiteAdministrationControllerRouteName = "SiteAdministration";
  }

	public static class ModelBinderConstants
	{
		public const string DeviceQueryModelBinderError = "DeviceQueryModelBinderError";
	}
}
