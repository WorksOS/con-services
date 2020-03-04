using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Config
{
    public abstract class AssetServiceMySqlQueries
    {
        static string database = AssetServiceConfig.MySqlDBName;
        public static string AssetDetailsByAssetUID = "SELECT AssetName, LegacyAssetID, SerialNumber, MakeCode, Model, AssetTypeName, IconKey, EquipmentVIN, ModelYear, hex(OwningCustomerUID),ObjectType,Category,ProjectStatus,SortField,Source,UserEnteredRuntimeHours,Classification  FROM `" + database + "`.Asset where hex(AssetUID) ='";
        public static string AssetUpdatedDetailsByAssetUID = "SELECT AssetName, LegacyAssetID, Model, AssetTypeName, IconKey, EquipmentVIN, ModelYear,ObjectType,Category,ProjectStatus,SortField,Source,UserEnteredRuntimeHours,Classification  FROM `" + database + "`.Asset where hex(AssetUID) ='";
        public static string AssetDeletedDetailsByAssetUID = "SELECT StatusInd  FROM `" + database + "`.Asset where hex(AssetUID) ='";
        public static string AssetDetails = "SELECT hex(AssetUID) FROM`" + database + "`.Asset limit 1";
        public static string MileageTargetAPI = "SELECT * FROM `VSS-MasterData-Asset-Dev`.Asset where fk_AssetUID in {'0'}";
        public static string Select_Assets= "   SELECT HEX(AssetUID) FROM `" + database + "`.Asset INNER JOIN AssetCustomer ac ON AssetUID = ac.fk_AssetUID WHERE ac.fk_CustomerUID = UNHEX(replace('{0}','-',''));";


    }

    public abstract class AssetSettingsMySqlQueries
    {
        static string database = AssetServiceConfig.MySqlDBName;
        public static string DeleteAssetWeeklyConfig = "DELETE FROM `" + database + "`.AssetWeeklyConfig where fk_AssetUID=unhex('{0}');";
       
    }
}
