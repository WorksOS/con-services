using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.AssetRepository.MySql.ColumnName
{
    public static class AssetCustomer
    {
        internal static class ColumnName
        {
            public const string ASSET_UID = "fk_AssetUID";
            public const string CUSTOMER_UID = "fk_CustomerUID";
            public const string ASSET_CUSTOMER_ID = "AssetCustomerID";
            public const string LAST_ASSET_CUSTOMER_UTC = "LastCustomerUTC";
        }
    }
}