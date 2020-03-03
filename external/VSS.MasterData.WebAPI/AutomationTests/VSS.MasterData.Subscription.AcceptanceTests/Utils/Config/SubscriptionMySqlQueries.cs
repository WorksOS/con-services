using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Config
{
    public abstract class SubscriptionMySqlQueries
    {
        public static string AssetSubscriptionDetailsByAssetSubscriptionUID = "SELECT fk_AssetUID, fk_DeviceUID, StartDate, EndDate  FROM `VSS-MasterData-Subscription-Dev`.AssetSubscription where AssetSubscriptionUID='";
        public static string CustomerSubscriptionDetailsByCustomerUID = "SELECT  fk_CustomerUID, b.Name, StartDate, EndDate   FROM `VSS-MasterData-Subscription-Dev`.CustomerSubscription a inner join `VSS-MasterData-Subscription-Dev`.ServiceType b on a.fk_ServiceTypeID=b.ServiceTypeID where fk_CustomerUID='";
        public static string ProjectSubscriptionDetailsByProjectSubscriptionUID = "SELECT fk_ProjectUID, StartDate, EndDate  FROM `VSS-MasterData-Subscription-Dev`.ProjectSubscription where ProjectSubscriptionUID='";
        public static string AssetSubscriptionDetails = "Select COUNT(1) from AssetSubscription where AssetSubscriptionUID = unhex('{0}') AND fk_AssetUID = unhex('{1}') AND fk_DeviceUID = unhex('{2}') AND StartDate = '{3}' AND EndDate = '{4}' AND fk_CustomerUID = unhex('{5}') AND fk_ServiceTypeID = '{6}' AND fk_SubscriptionSourceID = '{7}';";

    }
}

