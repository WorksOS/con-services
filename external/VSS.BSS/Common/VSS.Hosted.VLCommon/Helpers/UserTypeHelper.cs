using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Hosted.VLCommon.Helpers
{
  public static class UserTypeHelper
  {
    public static List<int> VlApifeatureTypes = new List<int>
      {
        (int)FeatureEnum.DataServices,
        (int)FeatureEnum.FarmWorksService,
        (int)FeatureEnum.AEMPService,
        (int)FeatureEnum.FeedServices,
        (int)FeatureEnum.StartStopService,
        (int)FeatureEnum.FenceAlertService,
        (int)FeatureEnum.FuelService,
        (int)FeatureEnum.EventService,
        (int)FeatureEnum.DiagnosticService,
        (int)FeatureEnum.EngineParametersService,
        (int)FeatureEnum.DigitalSwitchStatusService,
        (int)FeatureEnum.SecurityService,
        (int)FeatureEnum.SMULocationService,
        (int)FeatureEnum.MachineData,
        (int)FeatureEnum.RFIDData,
        (int)FeatureEnum.VLReadyAPI,
        (int)FeatureEnum.QueryAPI,
        (int)FeatureEnum.DataIn,
        (int)FeatureEnum.StoreProvisioning
              
      };

    public static List<int> VlSupportfeatureTypes = new List<int>
      {
        (int)FeatureEnum.NHAdmin,
        (int)FeatureEnum.OMT,
        (int)FeatureEnum.TrimbleDevices,
        (int)FeatureEnum.VLAdmin,
        (int)FeatureEnum.VLAdminTesting,
        (int)FeatureEnum.BSSScripting,
        (int)FeatureEnum.Provisioning,
        (int)FeatureEnum.UserManagement,
        (int)FeatureEnum.BillableProvisioning,
        (int)FeatureEnum.VLSupport,
        (int)FeatureEnum.VLTier1Support,
        (int)FeatureEnum.AssetDetails,
        (int)FeatureEnum.UserMove,
         (int)FeatureEnum.ViewModifyUsers,
        (int)FeatureEnum.CreateAPIUsers,
        (int)FeatureEnum.Migration,
        (int)FeatureEnum.VLSupportAlerts,
        (int)FeatureEnum.OculusSupport,
        (int)FeatureEnum.OculusProvisioning
      };

    public static List<int> ApiFeatureTypesWithCustomerNameInUrl = new List<int>
      {
        (int)FeatureEnum.StartStopService,
        (int)FeatureEnum.FenceAlertService,
        (int)FeatureEnum.FuelService,
        (int)FeatureEnum.EventService,
        (int)FeatureEnum.DiagnosticService,
        (int)FeatureEnum.EngineParametersService,
        (int)FeatureEnum.DigitalSwitchStatusService,
        (int)FeatureEnum.SecurityService,
        (int)FeatureEnum.SMULocationService,
      };
  }
}
