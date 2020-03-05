using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.AssetRepository.MySql.ColumnName
{
    #region AssetOwner

    public static class AssetOwner
    {
        internal static class ColumnName
        {
            public const string ASSET_UID = "fk_AssetUID";
            public const string CUSTOMER_UID = "fk_CustomerUID";
            public const string ACCOUNT_UID = "fk_AccountCustomerUID";
            public const string DEALER_UID = "fk_DealerCustomerUID";
            public const string NETWORK_CUSTOMER_CODE = "NetworkCustomerCode";
            public const string DEALER_ACCOUNT_CODE = "DealerAccountCode";
            public const string NETWORK_DEALER_CODE = "NetworkDealerCode";
            public const string ACCOUNT_NAME = "AccountName";
            public const string DEALER_NAME = "DealerName";
            public const string CUSTOMER_NAME = "CustomerName";
        }
    }

    #endregion
}
