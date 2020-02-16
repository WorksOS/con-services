using VSS.Hosted.VLCommon.Services;
using VSS.UnitTest.Common.EntityBuilder;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common
{
  public class Entity
  {
    #region APPFEATURESETAPPFEATURE

    public static AppFeatureSetAppFeatureBuilder AppFeatureSetAppFeature
    {
      get
      {
        return new AppFeatureSetAppFeatureBuilder();
      }
    }

    #endregion

    #region ASSETS
    public static AssetBuilder Asset
    {
      get { return new AssetBuilder(); }
    }
    #endregion

    #region Contacts
    public static ContactBuilder Contact
    {
      get { return new ContactBuilder(); }
    }
    #endregion

    #region CUSTOMERS
    public class Customer
    {
      public static CustomerBuilder Corporate
      {
        get { return new CustomerBuilder(CustomerTypeEnum.Corporate); }
      }
      public static CustomerBuilder Dealer
      {
        get { return new CustomerBuilder(CustomerTypeEnum.Dealer); }
      }
      public static CustomerBuilder EndCustomer
      {
        get { return new CustomerBuilder(CustomerTypeEnum.Customer); }
      }
      public static CustomerBuilder Account
      {
        get { return new CustomerBuilder(CustomerTypeEnum.Account); }
      }
      public static CustomerBuilder Administrator
      {
        get { return new CustomerBuilder(CustomerTypeEnum.Operations); }
      }
    }
    #endregion

    #region DEVICES
    public class Device
    {
      public static DeviceBuilder DeviceType(DeviceTypeEnum deviceType)
      {
        return new DeviceBuilder(deviceType);
      }
      public static DeviceBuilder NoDevice
      {
        get { return new DeviceBuilder(DeviceTypeEnum.MANUALDEVICE).GpsDeviceId(""); }
      }
      public static DeviceBuilder PL420
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL420); }
      }
      public static DeviceBuilder TM3000
      {
        get { return new DeviceBuilder(DeviceTypeEnum.TM3000); }
      }
      public static DeviceBuilder TAP66
      {
        get { return new DeviceBuilder(DeviceTypeEnum.TAP66); }
      }
      public static DeviceBuilder PL421
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL421); }
      }
      public static DeviceBuilder PL431
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL431); }
      }
      public static DeviceBuilder SNM451
      {
        get { return new DeviceBuilder(DeviceTypeEnum.SNM451); }
      }

      public static DeviceBuilder MTS521
      {
        get { return new DeviceBuilder(DeviceTypeEnum.Series521); }
      }
      public static DeviceBuilder MTS522
      {
        get { return new DeviceBuilder(DeviceTypeEnum.Series522); }
      }
      public static DeviceBuilder MTS523
      {
        get { return new DeviceBuilder(DeviceTypeEnum.Series523); }
      }
      public static DeviceBuilder SNM940
      {
        get { return new DeviceBuilder(DeviceTypeEnum.SNM940); }
      }
      public static DeviceBuilder SNM941
      {
        get { return new DeviceBuilder(DeviceTypeEnum.SNM941); }
      }
      public static DeviceBuilder PL321
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL321); }
      }
      public static DeviceBuilder PL121
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL121); }
      }
      public static DeviceBuilder TrimTrac
      {
        get { return new DeviceBuilder(DeviceTypeEnum.TrimTrac); }
      }
      public static DeviceBuilder CrossCheck
      {
        get { return new DeviceBuilder(DeviceTypeEnum.CrossCheck); }
      }
      public static DeviceBuilder PL641
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL641); }
      }
      public static DeviceBuilder PLE641
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PLE641); }
      }
      public static DeviceBuilder DCM300
      {
        get { return new DeviceBuilder(DeviceTypeEnum.DCM300); }
      }
      public static DeviceBuilder PL631
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL631); }
      }
      public static DeviceBuilder PLE631
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PLE631); }
      }
      public static DeviceBuilder PLE641PLUSPL631
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PLE641PLUSPL631); }
      }

      public static DeviceBuilder PL131
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL131); }
      }

      public static DeviceBuilder PL141
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL141); }
      }
      public static DeviceBuilder PL161
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL161); }
      }

      public static DeviceBuilder PL440
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL440); }
      }
      public static DeviceBuilder PL240
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL240); }
      }
      public static DeviceBuilder PL542
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PL542); }
      }
      public static DeviceBuilder PLE642
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PLE642); }
      }
      public static DeviceBuilder PLE742
      {
        get { return new DeviceBuilder(DeviceTypeEnum.PLE742); }
      }
      public static DeviceBuilder PL240B
      {
          get { return new DeviceBuilder(DeviceTypeEnum.PL240B); }
      }
    }

    public class DevicePersonality
    {
      public static DevicePersonalityBuilder SoftwareVersion
      {
        get { return new DevicePersonalityBuilder().PersonalityType(PersonalityTypeEnum.Software).Value("1.2.3"); }
      }
      public static DevicePersonalityBuilder GatewayVersion
      {
        get { return new DevicePersonalityBuilder().PersonalityType(PersonalityTypeEnum.Gateway).Value("0.0.1"); }
      }

      public static DevicePersonalityBuilder PL321VIMSModuleType
      {
        get { return new DevicePersonalityBuilder().PersonalityType(PersonalityTypeEnum.PL321ModuleType).Value("PL321VIMS"); }
      }

      public static DevicePersonalityBuilder PL321SRModuleType
      {
        get { return new DevicePersonalityBuilder().PersonalityType(PersonalityTypeEnum.PL321ModuleType).Value("PL121SR"); }
      }
    }

    #endregion

    #region RELATIONSHIPS
    public class CustomerRelationship
    {
      public static RelationshipBuilder Relate(VSS.Hosted.VLCommon.Customer parent, VSS.Hosted.VLCommon.Customer child)
      {
        return new RelationshipBuilder(parent, child);
      }
    }

    #endregion

    #region PRODUCT FAMILIES

    public static ProductFamilyBuilder ProductFamily
    {
      get
      {
        return new ProductFamilyBuilder();
      }
    }

    #endregion

    #region SALESMODELS

    public static SalesModelBuilder SalesModel
    {
      get
      {
        return new SalesModelBuilder();
      }
    }

    #endregion

    #region Icons

    public static IconBuilder Icon
    {
      get
      {
        return new IconBuilder();
      }
    }

    #endregion

    #region SERVICE PLANS
    public class Service
    {
      public static ServiceBuilder ServiceType(ServiceTypeEnum servicePlanEnum)
      {
        return new ServiceBuilder(servicePlanEnum);
      }
      public static ServiceBuilder ManualWatch
      {
        get { return new ServiceBuilder(ServiceTypeEnum.ManualMaintenanceLog); }
      }
      public static ServiceBuilder Essentials
      {
        get { return new ServiceBuilder(ServiceTypeEnum.Essentials); }
      }
      public static ServiceBuilder Health
      {
        get { return new ServiceBuilder(ServiceTypeEnum.StandardHealth); }
      }
      public static ServiceBuilder PussyHealth
      {
        get { return new ServiceBuilder(ServiceTypeEnum.CATHealth); }
      }
      public static ServiceBuilder Utilization
      {
        get { return new ServiceBuilder(ServiceTypeEnum.StandardUtilization); }
      }
      public static ServiceBuilder OneMinuteRate
      {
        get { return new ServiceBuilder(ServiceTypeEnum.e1minuteUpdateRateUpgrade); }
      }
      public static ServiceBuilder Maintenance
      {
        get { return new ServiceBuilder(ServiceTypeEnum.VLMAINT); }
      }
      public static ServiceBuilder TwoDProjectMonitoring
      {
        get { return new ServiceBuilder(ServiceTypeEnum.e2DProjectMonitoring); }
      }
      public static ServiceBuilder ThreeDProjectMonitoring
      {
        get { return new ServiceBuilder(ServiceTypeEnum.e3DProjectMonitoring); }
      }
      public static ServiceBuilder Landfill
      {
        get { return new ServiceBuilder(ServiceTypeEnum.Landfill); }
      }
      public static ServiceBuilder ProjectMonitoring
      {
        get { return new ServiceBuilder(ServiceTypeEnum.ProjectMonitoring); }
      }
    }
    #endregion

    #region SITES

    public static SiteBuilder Site
    {
      get
      {
        return new SiteBuilder(SiteTypeEnum.Generic);
      }
    }

    #endregion

    #region USERS
    public static UserBuilder User
    {
      get
      {
        return new UserBuilder();
      }
    }
    #endregion

    #region ACTIVE USERS

    public static ActiveUserBuilder ActiveUser
    {
      get
      {
        return new ActiveUserBuilder();
      }
    }

    #endregion

    #region ASSETEXPECTEDRUNTIMEPROJECTED
    public static AssetExpectedRuntimeHoursProjectedBuilder AssetExpectedRuntimeHoursProjected
    {
      get { return new AssetExpectedRuntimeHoursProjectedBuilder(); }
    }
    #endregion

    #region ASSETWORKINGDEFINITION
    public static AssetWorkingDefinitionBuilder AssetWorkingDefinition
    {
      get { return new AssetWorkingDefinitionBuilder(); }
    }
    #endregion

    #region ASSETBURNRATES
    public static AssetBurnRatesBuilder AssetBurnRates
    {
      get { return new AssetBurnRatesBuilder(); }
    }
    #endregion

   #region SALESMODELS

    public static PMSalesModelBuilder PMSalesModel
    {
      get
      {
        return new PMSalesModelBuilder();
      }
    }

    #endregion

    #region PMINTERVALS
    public static PMIntervalBuilder PMInterval
    {
      get { return new PMIntervalBuilder(); }
    }

    #endregion

    #region PMINTERVALASSETS
    public static PMIntervalAssetBuilder PMIntervalAsset
    {
      get { return new PMIntervalAssetBuilder(); }
    }

    #endregion

    #region MAKE
    public static MakeBuilder Make
    {
      get { return new MakeBuilder(); }
    }
    #endregion


    #region LANGUAGE
    public static LanguageBuilder Language
    {
      get { return new LanguageBuilder(); }
    }
    #endregion

    #region SERVICE PROVIDER
    public static ServiceProviderBuilder ServiceProvider
    {
      get { return new ServiceProviderBuilder(); }
    }

    #endregion

    #region ECMInfo
    public static ECMInfoBuilder ECMInfo
    {
      get { return new ECMInfoBuilder(); }
    }

    #endregion

    #region ECMDatalinkInfo
    public static ECMDataLinkInfoBuilder ECMDatalinkInfo
    {
      get { return new ECMDataLinkInfoBuilder(); }
    }

    #endregion

    #region MID
    public static MIDBuilder MID
    {
      get { return new MIDBuilder(); }
    }

    #endregion
  }
}
