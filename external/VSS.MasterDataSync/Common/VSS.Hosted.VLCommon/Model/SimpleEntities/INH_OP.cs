//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// Architectural overview and usage guide: 
// http://blogofrab.blogspot.com/2010/08/maintenance-free-mocking-for-unit.html
//------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;

namespace VSS.Hosted.VLCommon
{
    /// <summary>
    /// The interface for the specialised object context. This contains all of
    /// the <code>ObjectSet</code> properties that are implemented in both the
    /// functional context class and the mock context class.
    /// </summary>
    public interface INH_OP : System.IDisposable
    {
        IObjectSet<AlertFault> AlertFault { get; }
        IObjectSet<AlertFault> AlertFaultReadOnly { get; }
        IObjectSet<AlertIncident> AlertIncident { get; }
        IObjectSet<AlertIncident> AlertIncidentReadOnly { get; }
        IObjectSet<AlertParameter> AlertParameter { get; }
        IObjectSet<AlertParameter> AlertParameterReadOnly { get; }
        IObjectSet<AlertParameterType> AlertParameterType { get; }
        IObjectSet<AlertParameterType> AlertParameterTypeReadOnly { get; }
        IObjectSet<AlertZones> AlertZones { get; }
        IObjectSet<AlertZones> AlertZonesReadOnly { get; }
        IObjectSet<AppAlarm> AppAlarm { get; }
        IObjectSet<AppAlarm> AppAlarmReadOnly { get; }
        IObjectSet<Asset> Asset { get; }
        IObjectSet<Asset> AssetReadOnly { get; }
        IObjectSet<AssetGroup> AssetGroup { get; }
        IObjectSet<AssetGroup> AssetGroupReadOnly { get; }
        IObjectSet<AssetOperator> AssetOperator { get; }
        IObjectSet<AssetOperator> AssetOperatorReadOnly { get; }
        IObjectSet<BookmarkManager> BookmarkManager { get; }
        IObjectSet<BookmarkManager> BookmarkManagerReadOnly { get; }
        IObjectSet<BSSMessages> BSSMessages { get; }
        IObjectSet<BSSMessages> BSSMessagesReadOnly { get; }
        IObjectSet<ConfigType> ConfigType { get; }
        IObjectSet<ConfigType> ConfigTypeReadOnly { get; }
        IObjectSet<Contact> Contact { get; }
        IObjectSet<Contact> ContactReadOnly { get; }
        IObjectSet<CoordinateSystem> CoordinateSystem { get; }
        IObjectSet<CoordinateSystem> CoordinateSystemReadOnly { get; }
        IObjectSet<CrosscheckConfig> CrosscheckConfig { get; }
        IObjectSet<CrosscheckConfig> CrosscheckConfigReadOnly { get; }
        IObjectSet<Customer> Customer { get; }
        IObjectSet<Customer> CustomerReadOnly { get; }
        IObjectSet<CustomerRelationship> CustomerRelationship { get; }
        IObjectSet<CustomerRelationship> CustomerRelationshipReadOnly { get; }
        IObjectSet<CustomerType> CustomerType { get; }
        IObjectSet<CustomerType> CustomerTypeReadOnly { get; }
        IObjectSet<Datalink> Datalink { get; }
        IObjectSet<Datalink> DatalinkReadOnly { get; }
        IObjectSet<DealerNetwork> DealerNetwork { get; }
        IObjectSet<DealerNetwork> DealerNetworkReadOnly { get; }
        IObjectSet<Device> Device { get; }
        IObjectSet<Device> DeviceReadOnly { get; }
        IObjectSet<DeviceFirmwareVersion> DeviceFirmwareVersion { get; }
        IObjectSet<DeviceFirmwareVersion> DeviceFirmwareVersionReadOnly { get; }
        IObjectSet<DeviceState> DeviceState { get; }
        IObjectSet<DeviceState> DeviceStateReadOnly { get; }
        IObjectSet<DeviceType> DeviceType { get; }
        IObjectSet<DeviceType> DeviceTypeReadOnly { get; }
        IObjectSet<ECMDatalinkInfo> ECMDatalinkInfo { get; }
        IObjectSet<ECMDatalinkInfo> ECMDatalinkInfoReadOnly { get; }
        IObjectSet<ECMInfo> ECMInfo { get; }
        IObjectSet<ECMInfo> ECMInfoReadOnly { get; }
        IObjectSet<ExternalUser> ExternalUser { get; }
        IObjectSet<ExternalUser> ExternalUserReadOnly { get; }
        IObjectSet<Fault> Fault { get; }
        IObjectSet<Fault> FaultReadOnly { get; }
        IObjectSet<FaultDescription> FaultDescription { get; }
        IObjectSet<FaultDescription> FaultDescriptionReadOnly { get; }
        IObjectSet<FaultType> FaultType { get; }
        IObjectSet<FaultType> FaultTypeReadOnly { get; }
        IObjectSet<FeatureAccess> FeatureAccess { get; }
        IObjectSet<FeatureAccess> FeatureAccessReadOnly { get; }
        IObjectSet<FirmwareUpdateStatus> FirmwareUpdateStatus { get; }
        IObjectSet<FirmwareUpdateStatus> FirmwareUpdateStatusReadOnly { get; }
        IObjectSet<Icon> Icon { get; }
        IObjectSet<Icon> IconReadOnly { get; }
        IObjectSet<ImportedFile> ImportedFile { get; }
        IObjectSet<ImportedFile> ImportedFileReadOnly { get; }
        IObjectSet<Language> Language { get; }
        IObjectSet<Language> LanguageReadOnly { get; }
        IObjectSet<Make> Make { get; }
        IObjectSet<Make> MakeReadOnly { get; }
        IObjectSet<MID> MID { get; }
        IObjectSet<MID> MIDReadOnly { get; }
        IObjectSet<MIDDesc> MIDDesc { get; }
        IObjectSet<MIDDesc> MIDDescReadOnly { get; }
        IObjectSet<MTS500FirmwareVersion> MTS500FirmwareVersion { get; }
        IObjectSet<MTS500FirmwareVersion> MTS500FirmwareVersionReadOnly { get; }
        IObjectSet<NighthawkSync> NighthawkSync { get; }
        IObjectSet<NighthawkSync> NighthawkSyncReadOnly { get; }
        IObjectSet<Notice> Notice { get; }
        IObjectSet<Notice> NoticeReadOnly { get; }
        IObjectSet<NoticeLanguage> NoticeLanguage { get; }
        IObjectSet<NoticeLanguage> NoticeLanguageReadOnly { get; }
        IObjectSet<PersonalityType> PersonalityType { get; }
        IObjectSet<PersonalityType> PersonalityTypeReadOnly { get; }
        IObjectSet<PMCATCheckListStep> PMCATCheckListStep { get; }
        IObjectSet<PMCATCheckListStep> PMCATCheckListStepReadOnly { get; }
        IObjectSet<PMCATInterval> PMCATInterval { get; }
        IObjectSet<PMCATInterval> PMCATIntervalReadOnly { get; }
        IObjectSet<PMCheckListStep> PMCheckListStep { get; }
        IObjectSet<PMCheckListStep> PMCheckListStepReadOnly { get; }
        IObjectSet<PMCompletedService> PMCompletedService { get; }
        IObjectSet<PMCompletedService> PMCompletedServiceReadOnly { get; }
        IObjectSet<PMCompletedServiceAuditHistory> PMCompletedServiceAuditHistory { get; }
        IObjectSet<PMCompletedServiceAuditHistory> PMCompletedServiceAuditHistoryReadOnly { get; }
        IObjectSet<PMIntervalAsset> PMIntervalAsset { get; }
        IObjectSet<PMIntervalAsset> PMIntervalAssetReadOnly { get; }
        IObjectSet<PMPart> PMPart { get; }
        IObjectSet<PMPart> PMPartReadOnly { get; }
        IObjectSet<PMServiceCompletionType> PMServiceCompletionType { get; }
        IObjectSet<PMServiceCompletionType> PMServiceCompletionTypeReadOnly { get; }
        IObjectSet<PMTrackingType> PMTrackingType { get; }
        IObjectSet<PMTrackingType> PMTrackingTypeReadOnly { get; }
        IObjectSet<ProductFamily> ProductFamily { get; }
        IObjectSet<ProductFamily> ProductFamilyReadOnly { get; }
        IObjectSet<Project> Project { get; }
        IObjectSet<Project> ProjectReadOnly { get; }
        IObjectSet<ProjectSetting> ProjectSetting { get; }
        IObjectSet<ProjectSetting> ProjectSettingReadOnly { get; }
        IObjectSet<Sensor> Sensor { get; }
        IObjectSet<Sensor> SensorReadOnly { get; }
        IObjectSet<Service> Service { get; }
        IObjectSet<Service> ServiceReadOnly { get; }
        IObjectSet<ServiceProvider> ServiceProvider { get; }
        IObjectSet<ServiceProvider> ServiceProviderReadOnly { get; }
        IObjectSet<ServiceType> ServiceType { get; }
        IObjectSet<ServiceType> ServiceTypeReadOnly { get; }
        IObjectSet<ServiceView> ServiceView { get; }
        IObjectSet<ServiceView> ServiceViewReadOnly { get; }
        IObjectSet<Site> Site { get; }
        IObjectSet<Site> SiteReadOnly { get; }
        IObjectSet<SiteDispatched> SiteDispatched { get; }
        IObjectSet<SiteDispatched> SiteDispatchedReadOnly { get; }
        IObjectSet<SiteType> SiteType { get; }
        IObjectSet<SiteType> SiteTypeReadOnly { get; }
        IObjectSet<TelematicsSync> TelematicsSync { get; }
        IObjectSet<TelematicsSync> TelematicsSyncReadOnly { get; }
        IObjectSet<TermsOfUse> TermsOfUse { get; }
        IObjectSet<TermsOfUse> TermsOfUseReadOnly { get; }
        IObjectSet<TopicPriority> TopicPriority { get; }
        IObjectSet<TopicPriority> TopicPriorityReadOnly { get; }
        IObjectSet<TopicScalePolicy> TopicScalePolicy { get; }
        IObjectSet<TopicScalePolicy> TopicScalePolicyReadOnly { get; }
        IObjectSet<User> User { get; }
        IObjectSet<User> UserReadOnly { get; }
        IObjectSet<UserActivation> UserActivation { get; }
        IObjectSet<UserActivation> UserActivationReadOnly { get; }
        IObjectSet<UserActivationStatus> UserActivationStatus { get; }
        IObjectSet<UserActivationStatus> UserActivationStatusReadOnly { get; }
        IObjectSet<UserFeature> UserFeature { get; }
        IObjectSet<UserFeature> UserFeatureReadOnly { get; }
        IObjectSet<UserNoticeDismissed> UserNoticeDismissed { get; }
        IObjectSet<UserNoticeDismissed> UserNoticeDismissedReadOnly { get; }
        IObjectSet<UserPasswordHistory> UserPasswordHistory { get; }
        IObjectSet<UserPasswordHistory> UserPasswordHistoryReadOnly { get; }
        IObjectSet<UserPreferences> UserPreferences { get; }
        IObjectSet<UserPreferences> UserPreferencesReadOnly { get; }
        IObjectSet<WorkDefinition> WorkDefinition { get; }
        IObjectSet<WorkDefinition> WorkDefinitionReadOnly { get; }
        IObjectSet<Zone> Zone { get; }
        IObjectSet<Zone> ZoneReadOnly { get; }
        IObjectSet<DeviceTypeServiceType> DeviceTypeServiceType { get; }
        IObjectSet<DeviceTypeServiceType> DeviceTypeServiceTypeReadOnly { get; }
        IObjectSet<AssetDeviceHistory> AssetDeviceHistory { get; }
        IObjectSet<AssetDeviceHistory> AssetDeviceHistoryReadOnly { get; }
        IObjectSet<DealerNetworkServiceType> DealerNetworkServiceType { get; }
        IObjectSet<DealerNetworkServiceType> DealerNetworkServiceTypeReadOnly { get; }
        IObjectSet<CustomerRelationshipType> CustomerRelationshipType { get; }
        IObjectSet<CustomerRelationshipType> CustomerRelationshipTypeReadOnly { get; }
        IObjectSet<ProjectDataFilter> ProjectDataFilter { get; }
        IObjectSet<ProjectDataFilter> ProjectDataFilterReadOnly { get; }
        IObjectSet<BSSProvisioningMsg> BSSProvisioningMsg { get; }
        IObjectSet<BSSProvisioningMsg> BSSProvisioningMsgReadOnly { get; }
        IObjectSet<BSSResponseEndPoint> BSSResponseEndPoint { get; }
        IObjectSet<BSSResponseEndPoint> BSSResponseEndPointReadOnly { get; }
        IObjectSet<BSSResponseMsg> BSSResponseMsg { get; }
        IObjectSet<BSSResponseMsg> BSSResponseMsgReadOnly { get; }
        IObjectSet<BSSStatus> BSSStatus { get; }
        IObjectSet<BSSStatus> BSSStatusReadOnly { get; }
        IObjectSet<VLAdminErrorMsg> VLAdminErrorMsg { get; }
        IObjectSet<VLAdminErrorMsg> VLAdminErrorMsgReadOnly { get; }
        IObjectSet<AssetOnboardingProcess> AssetOnboardingProcess { get; }
        IObjectSet<AssetOnboardingProcess> AssetOnboardingProcessReadOnly { get; }
        IObjectSet<AssetOnboardingProcessCompleted> AssetOnboardingProcessCompleted { get; }
        IObjectSet<AssetOnboardingProcessCompleted> AssetOnboardingProcessCompletedReadOnly { get; }
        IObjectSet<DeviceICDSeries> DeviceICDSeries { get; }
        IObjectSet<DeviceICDSeries> DeviceICDSeriesReadOnly { get; }
        IObjectSet<DevicePartNumber> DevicePartNumber { get; }
        IObjectSet<DevicePartNumber> DevicePartNumberReadOnly { get; }
        IObjectSet<FaultParameter> FaultParameter { get; }
        IObjectSet<FaultParameter> FaultParameterReadOnly { get; }
        IObjectSet<FaultParameterType> FaultParameterType { get; }
        IObjectSet<FaultParameterType> FaultParameterTypeReadOnly { get; }
        IObjectSet<AssetSecurityIncident> AssetSecurityIncident { get; }
        IObjectSet<AssetSecurityIncident> AssetSecurityIncidentReadOnly { get; }
        IObjectSet<ActiveUserAssetSelection> ActiveUserAssetSelection { get; }
        IObjectSet<ActiveUserAssetSelection> ActiveUserAssetSelectionReadOnly { get; }
        IObjectSet<vw_ServiceView> vw_ServiceView { get; }
        IObjectSet<vw_ServiceView> vw_ServiceViewReadOnly { get; }
        IObjectSet<ActiveUser> ActiveUser { get; }
        IObjectSet<ActiveUser> ActiveUserReadOnly { get; }
        IObjectSet<vw_DealerOwnedAssets> vw_DealerOwnedAssets { get; }
        IObjectSet<vw_DealerOwnedAssets> vw_DealerOwnedAssetsReadOnly { get; }
        IObjectSet<vw_ParentCustomerOwnedAssets> vw_ParentCustomerOwnedAssets { get; }
        IObjectSet<vw_ParentCustomerOwnedAssets> vw_ParentCustomerOwnedAssetsReadOnly { get; }
        IObjectSet<CustomerAsset> CustomerAsset { get; }
        IObjectSet<CustomerAsset> CustomerAssetReadOnly { get; }
        IObjectSet<vw_AssetWorkingSet> vw_AssetWorkingSet { get; }
        IObjectSet<vw_AssetWorkingSet> vw_AssetWorkingSetReadOnly { get; }
        IObjectSet<UserGroupFavorites> UserGroupFavorites { get; }
        IObjectSet<UserGroupFavorites> UserGroupFavoritesReadOnly { get; }
        IObjectSet<UserSiteFavorites> UserSiteFavorites { get; }
        IObjectSet<UserSiteFavorites> UserSiteFavoritesReadOnly { get; }
        IObjectSet<AlertContact> AlertContact { get; }
        IObjectSet<AlertContact> AlertContactReadOnly { get; }
        IObjectSet<AlertAssetGroup> AlertAssetGroup { get; }
        IObjectSet<AlertAssetGroup> AlertAssetGroupReadOnly { get; }
        IObjectSet<AssetGroupAsset> AssetGroupAsset { get; }
        IObjectSet<AssetGroupAsset> AssetGroupAssetReadOnly { get; }
        IObjectSet<AlertSite> AlertSite { get; }
        IObjectSet<AlertSite> AlertSiteReadOnly { get; }
        IObjectSet<AlertAsset> AlertAsset { get; }
        IObjectSet<AlertAsset> AlertAssetReadOnly { get; }
        IObjectSet<MessageSequence> MessageSequence { get; }
        IObjectSet<MessageSequence> MessageSequenceReadOnly { get; }
        IObjectSet<DriverIDConfig> DriverIDConfig { get; }
        IObjectSet<DriverIDConfig> DriverIDConfigReadOnly { get; }
        IObjectSet<DXFUnitsType> DXFUnitsType { get; }
        IObjectSet<DXFUnitsType> DXFUnitsTypeReadOnly { get; }
        IObjectSet<ImportedFileType> ImportedFileType { get; }
        IObjectSet<ImportedFileType> ImportedFileTypeReadOnly { get; }
        IObjectSet<AlertNote> AlertNote { get; }
        IObjectSet<AlertNote> AlertNoteReadOnly { get; }
        IObjectSet<AssetCycle> AssetCycle { get; }
        IObjectSet<AssetCycle> AssetCycleReadOnly { get; }
        IObjectSet<Cycle> Cycle { get; }
        IObjectSet<Cycle> CycleReadOnly { get; }
        IObjectSet<PMInterval> PMInterval { get; }
        IObjectSet<PMInterval> PMIntervalReadOnly { get; }
        IObjectSet<UserAlertFavorites> UserAlertFavorites { get; }
        IObjectSet<UserAlertFavorites> UserAlertFavoritesReadOnly { get; }
        IObjectSet<Alert> Alert { get; }
        IObjectSet<Alert> AlertReadOnly { get; }
        IObjectSet<vw_Address> vw_Address { get; }
        IObjectSet<vw_Address> vw_AddressReadOnly { get; }
        IObjectSet<AssetBurnRates> AssetBurnRates { get; }
        IObjectSet<AssetBurnRates> AssetBurnRatesReadOnly { get; }
        IObjectSet<AssetExpectedRuntimeHoursHistoric> AssetExpectedRuntimeHoursHistoric { get; }
        IObjectSet<AssetExpectedRuntimeHoursHistoric> AssetExpectedRuntimeHoursHistoricReadOnly { get; }
        IObjectSet<AssetExpectedRuntimeHoursProjected> AssetExpectedRuntimeHoursProjected { get; }
        IObjectSet<AssetExpectedRuntimeHoursProjected> AssetExpectedRuntimeHoursProjectedReadOnly { get; }
        IObjectSet<AssetWorkingDefinition> AssetWorkingDefinition { get; }
        IObjectSet<AssetWorkingDefinition> AssetWorkingDefinitionReadOnly { get; }
        IObjectSet<vw_FactAssetCustomerSite> vw_FactAssetCustomerSite { get; }
        IObjectSet<vw_FactAssetCustomerSite> vw_FactAssetCustomerSiteReadOnly { get; }
        IObjectSet<AlertAssetOccupancySite> AlertAssetOccupancySite { get; }
        IObjectSet<AlertAssetOccupancySite> AlertAssetOccupancySiteReadOnly { get; }
        IObjectSet<TemperatureUnit> TemperatureUnit { get; }
        IObjectSet<TemperatureUnit> TemperatureUnitReadOnly { get; }
        IObjectSet<ProjectSiteSetting> ProjectSiteSetting { get; }
        IObjectSet<ProjectSiteSetting> ProjectSiteSettingReadOnly { get; }
        IObjectSet<ProjectSiteUser> ProjectSiteUser { get; }
        IObjectSet<ProjectSiteUser> ProjectSiteUserReadOnly { get; }
        IObjectSet<DailyReport> DailyReport { get; }
        IObjectSet<DailyReport> DailyReportReadOnly { get; }
        IObjectSet<J1939DefaultMIDDescription> J1939DefaultMIDDescription { get; }
        IObjectSet<J1939DefaultMIDDescription> J1939DefaultMIDDescriptionReadOnly { get; }
        IObjectSet<Hierarchy> Hierarchy { get; }
        IObjectSet<Hierarchy> HierarchyReadOnly { get; }
        IObjectSet<HierNodeAssoc> HierNodeAssoc { get; }
        IObjectSet<HierNodeAssoc> HierNodeAssocReadOnly { get; }
        IObjectSet<NodeType> NodeType { get; }
        IObjectSet<NodeType> NodeTypeReadOnly { get; }
        IObjectSet<ModelVariant> ModelVariant { get; }
        IObjectSet<ModelVariant> ModelVariantReadOnly { get; }
        IObjectSet<PMIntervalInstance> PMIntervalInstance { get; }
        IObjectSet<PMIntervalInstance> PMIntervalInstanceReadOnly { get; }
        IObjectSet<FederatedLogonInfo> FederatedLogonInfo { get; }
        IObjectSet<FederatedLogonInfo> FederatedLogonInfoReadOnly { get; }
        IObjectSet<SharedAsset> SharedAsset { get; }
        IObjectSet<SharedAsset> SharedAssetReadOnly { get; }
        IObjectSet<SharedViewNote> SharedViewNote { get; }
        IObjectSet<SharedViewNote> SharedViewNoteReadOnly { get; }
        IObjectSet<ImportedFileHistory> ImportedFileHistory { get; }
        IObjectSet<ImportedFileHistory> ImportedFileHistoryReadOnly { get; }
        IObjectSet<SharedCustomer> SharedCustomer { get; }
        IObjectSet<SharedCustomer> SharedCustomerReadOnly { get; }
        IObjectSet<SharedView> SharedView { get; }
        IObjectSet<SharedView> SharedViewReadOnly { get; }
        IObjectSet<MassHaulPlan> MassHaulPlan { get; }
        IObjectSet<MassHaulPlan> MassHaulPlanReadOnly { get; }
        IObjectSet<AssetReposessionHistory> AssetReposessionHistory { get; }
        IObjectSet<AssetReposessionHistory> AssetReposessionHistoryReadOnly { get; }
        IObjectSet<HierHistory> HierHistory { get; }
        IObjectSet<HierHistory> HierHistoryReadOnly { get; }
        IObjectSet<DeviceIPConfig> DeviceIPConfig { get; }
        IObjectSet<DeviceIPConfig> DeviceIPConfigReadOnly { get; }
        IObjectSet<DevicePersonality> DevicePersonality { get; }
        IObjectSet<DevicePersonality> DevicePersonalityReadOnly { get; }
        IObjectSet<AssetMonitoring> AssetMonitoring { get; }
        IObjectSet<AssetMonitoring> AssetMonitoringReadOnly { get; }
        IObjectSet<MonitoringMachineType> MonitoringMachineType { get; }
        IObjectSet<MonitoringMachineType> MonitoringMachineTypeReadOnly { get; }
        IObjectSet<MassHaulPlanLine> MassHaulPlanLine { get; }
        IObjectSet<MassHaulPlanLine> MassHaulPlanLineReadOnly { get; }
        IObjectSet<Tz_TimeZone> Tz_TimeZone { get; }
        IObjectSet<Tz_TimeZone> Tz_TimeZoneReadOnly { get; }
        IObjectSet<Tz_World_MP> Tz_World_MP { get; }
        IObjectSet<Tz_World_MP> Tz_World_MPReadOnly { get; }
        IObjectSet<Tz_DaylightSavingDateRule> Tz_DaylightSavingDateRule { get; }
        IObjectSet<Tz_DaylightSavingDateRule> Tz_DaylightSavingDateRuleReadOnly { get; }
        IObjectSet<Tz_DayLightYearly> Tz_DayLightYearly { get; }
        IObjectSet<Tz_DayLightYearly> Tz_DayLightYearlyReadOnly { get; }
        IObjectSet<AssetAlias> AssetAlias { get; }
        IObjectSet<AssetAlias> AssetAliasReadOnly { get; }
        IObjectSet<EmailPriority> EmailPriority { get; }
        IObjectSet<EmailPriority> EmailPriorityReadOnly { get; }
        IObjectSet<EmailQueue> EmailQueue { get; }
        IObjectSet<EmailQueue> EmailQueueReadOnly { get; }
        IObjectSet<ScheduleReportContact> ScheduleReportContact { get; }
        IObjectSet<ScheduleReportContact> ScheduleReportContactReadOnly { get; }
        IObjectSet<ScheduleReportFrequency> ScheduleReportFrequency { get; }
        IObjectSet<ScheduleReportFrequency> ScheduleReportFrequencyReadOnly { get; }
        IObjectSet<SchedulerStatus> SchedulerStatus { get; }
        IObjectSet<SchedulerStatus> SchedulerStatusReadOnly { get; }
        IObjectSet<SMSQueue> SMSQueue { get; }
        IObjectSet<SMSQueue> SMSQueueReadOnly { get; }
        IObjectSet<ScheduleReportLocation> ScheduleReportLocation { get; }
        IObjectSet<ScheduleReportLocation> ScheduleReportLocationReadOnly { get; }
        IObjectSet<ScheduleReportTimeTableHistory> ScheduleReportTimeTableHistory { get; }
        IObjectSet<ScheduleReportTimeTableHistory> ScheduleReportTimeTableHistoryReadOnly { get; }
        IObjectSet<ScheduleReportAccess> ScheduleReportAccess { get; }
        IObjectSet<ScheduleReportAccess> ScheduleReportAccessReadOnly { get; }
        IObjectSet<ScheduleReportFileStatus> ScheduleReportFileStatus { get; }
        IObjectSet<ScheduleReportFileStatus> ScheduleReportFileStatusReadOnly { get; }
        IObjectSet<ScheduleReportTimeTable> ScheduleReportTimeTable { get; }
        IObjectSet<ScheduleReportTimeTable> ScheduleReportTimeTableReadOnly { get; }
        IObjectSet<ScheduleReport> ScheduleReport { get; }
        IObjectSet<ScheduleReport> ScheduleReportReadOnly { get; }
        IObjectSet<FaultDescriptionOverride> FaultDescriptionOverride { get; }
        IObjectSet<FaultDescriptionOverride> FaultDescriptionOverrideReadOnly { get; }
        IObjectSet<vw_FaultDescriptions> vw_FaultDescriptions { get; }
        IObjectSet<vw_FaultDescriptions> vw_FaultDescriptionsReadOnly { get; }
        IObjectSet<MassHaulBalance> MassHaulBalance { get; }
        IObjectSet<MassHaulBalance> MassHaulBalanceReadOnly { get; }
        IObjectSet<MassHaulBalanceZone> MassHaulBalanceZone { get; }
        IObjectSet<MassHaulBalanceZone> MassHaulBalanceZoneReadOnly { get; }
        IObjectSet<MassHaulMasterZone> MassHaulMasterZone { get; }
        IObjectSet<MassHaulMasterZone> MassHaulMasterZoneReadOnly { get; }
        IObjectSet<MassHaulWorkGroupType> MassHaulWorkGroupType { get; }
        IObjectSet<MassHaulWorkGroupType> MassHaulWorkGroupTypeReadOnly { get; }
        IObjectSet<SalesModel> SalesModel { get; }
        IObjectSet<SalesModel> SalesModelReadOnly { get; }
        IObjectSet<PressureUnit> PressureUnit { get; }
        IObjectSet<PressureUnit> PressureUnitReadOnly { get; }
        IObjectSet<VLSupportAudit> VLSupportAudit { get; }
        IObjectSet<VLSupportAudit> VLSupportAuditReadOnly { get; }
        IObjectSet<PMSalesModel> PMSalesModel { get; }
        IObjectSet<PMSalesModel> PMSalesModelReadOnly { get; }
        IObjectSet<OEMThemecolor> OEMThemecolor { get; }
        IObjectSet<OEMThemecolor> OEMThemecolorReadOnly { get; }
        IObjectSet<MassHaulRoute> MassHaulRoute { get; }
        IObjectSet<MassHaulRoute> MassHaulRouteReadOnly { get; }
        IObjectSet<AlertAssetAssociation> AlertAssetAssociation { get; }
        IObjectSet<AlertAssetAssociation> AlertAssetAssociationReadOnly { get; }
        IObjectSet<CustomerStore> CustomerStore { get; }
        IObjectSet<CustomerStore> CustomerStoreReadOnly { get; }
        IObjectSet<Store> Store { get; }
        IObjectSet<Store> StoreReadOnly { get; }
        IObjectSet<AlertAssetSelection> AlertAssetSelection { get; }
        IObjectSet<AlertAssetSelection> AlertAssetSelectionReadOnly { get; }
        IObjectSet<AssetReference> AssetReference { get; }
        IObjectSet<AssetReference> AssetReferenceReadOnly { get; }
        IObjectSet<CustomerReference> CustomerReference { get; }
        IObjectSet<CustomerReference> CustomerReferenceReadOnly { get; }
        IObjectSet<DeviceReference> DeviceReference { get; }
        IObjectSet<DeviceReference> DeviceReferenceReadOnly { get; }
        IObjectSet<ServiceReference> ServiceReference { get; }
        IObjectSet<ServiceReference> ServiceReferenceReadOnly { get; }
        IObjectSet<AppFeatureSet> AppFeatureSet { get; }
        IObjectSet<AppFeatureSet> AppFeatureSetReadOnly { get; }
        IObjectSet<AppFeatureSetAppFeature> AppFeatureSetAppFeature { get; }
        IObjectSet<AppFeatureSetAppFeature> AppFeatureSetAppFeatureReadOnly { get; }
        IObjectSet<AppFeature> AppFeature { get; }
        IObjectSet<AppFeature> AppFeatureReadOnly { get; }
        IObjectSet<RunTimeAdjustment> RunTimeAdjustment { get; }
        IObjectSet<RunTimeAdjustment> RunTimeAdjustmentReadOnly { get; }
        IObjectSet<MaterialType> MaterialType { get; }
        IObjectSet<MaterialType> MaterialTypeReadOnly { get; }
        IObjectSet<AssetSensorMaterialType> AssetSensorMaterialType { get; }
        IObjectSet<AssetSensorMaterialType> AssetSensorMaterialTypeReadOnly { get; }
        IObjectSet<AlertType> AlertType { get; }
        IObjectSet<AlertType> AlertTypeReadOnly { get; }
        IObjectSet<DeviceTypeSwitchMask> DeviceTypeSwitchMask { get; }
        IObjectSet<DeviceTypeSwitchMask> DeviceTypeSwitchMaskReadOnly { get; }
        IObjectSet<ServiceTypeAppFeature> ServiceTypeAppFeature { get; }
        IObjectSet<ServiceTypeAppFeature> ServiceTypeAppFeatureReadOnly { get; }
        IObjectSet<vw_Asset_AppFeature> vw_Asset_AppFeature { get; }
        IObjectSet<vw_Asset_AppFeature> vw_Asset_AppFeatureReadOnly { get; }
        IObjectSet<AlertDeviceTypeSwitchMask> AlertDeviceTypeSwitchMask { get; }
        IObjectSet<AlertDeviceTypeSwitchMask> AlertDeviceTypeSwitchMaskReadOnly { get; }
        IObjectSet<BookmarkManagerTagFile> BookmarkManagerTagFile { get; }
        IObjectSet<BookmarkManagerTagFile> BookmarkManagerTagFileReadOnly { get; }
        IObjectSet<ProductFamilyIcon> ProductFamilyIcon { get; }
        IObjectSet<ProductFamilyIcon> ProductFamilyIconReadOnly { get; }
        IObjectSet<TimeZoneStandardAbbreviation> TimeZoneStandardAbbreviation { get; }
        IObjectSet<TimeZoneStandardAbbreviation> TimeZoneStandardAbbreviationReadOnly { get; }
        IObjectSet<UnifyCustomerToken> UnifyCustomerToken { get; }
        IObjectSet<UnifyCustomerToken> UnifyCustomerTokenReadOnly { get; }
        IObjectSet<ProjectType> ProjectType { get; }
        IObjectSet<ProjectType> ProjectTypeReadOnly { get; }
        IObjectSet<Feature> Feature { get; }
        IObjectSet<Feature> FeatureReadOnly { get; }
        IObjectSet<FeatureType> FeatureType { get; }
        IObjectSet<FeatureType> FeatureTypeReadOnly { get; }
        IObjectSet<FeatureURLTemplate> FeatureURLTemplate { get; }
        IObjectSet<FeatureURLTemplate> FeatureURLTemplateReadOnly { get; }
        IObjectSet<MassHaulState> MassHaulState { get; }
        IObjectSet<MassHaulState> MassHaulStateReadOnly { get; }
        IObjectSet<ProjectService> ProjectService { get; }
        IObjectSet<ProjectService> ProjectServiceReadOnly { get; }
        IObjectSet<MassHaulZoneTransfer> MassHaulZoneTransfer { get; }
        IObjectSet<MassHaulZoneTransfer> MassHaulZoneTransferReadOnly { get; }
        IObjectSet<MasterDataSync> MasterDataSync { get; }
        IObjectSet<MasterDataSync> MasterDataSyncReadOnly { get; }
        IObjectSet<TDMGClaimedDevices> TDMGClaimedDevices { get; }
        IObjectSet<TDMGClaimedDevices> TDMGClaimedDevicesReadOnly { get; }
        IObjectSet<vw_CustomerIDsWithMaxAssetsForAirLift> vw_CustomerIDsWithMaxAssetsForAirLift { get; }
        IObjectSet<vw_CustomerIDsWithMaxAssetsForAirLift> vw_CustomerIDsWithMaxAssetsForAirLiftReadOnly { get; }
        IObjectSet<SubscriptionPlan> SubscriptionPlan { get; }
        IObjectSet<SubscriptionPlan> SubscriptionPlanReadOnly { get; }
        IObjectSet<SubscriptionMapping> SubscriptionMapping { get; }
        IObjectSet<SubscriptionMapping> SubscriptionMappingReadOnly { get; }
        IObjectSet<AEMPDataFeedURL> AEMPDataFeedURL { get; }
        IObjectSet<AEMPDataFeedURL> AEMPDataFeedURLReadOnly { get; }
        IObjectSet<ServiceTypeMap> ServiceTypeMap { get; }
        IObjectSet<ServiceTypeMap> ServiceTypeMapReadOnly { get; }
        IObjectSet<CustomerRelationshipExport> CustomerRelationshipExport { get; }
        IObjectSet<CustomerRelationshipExport> CustomerRelationshipExportReadOnly { get; }
      System.Data.Common.DbConnection Connection { get; }
    	int SaveChanges();
    }
}
