using System;
using System.Linq;
using log4net;
using VSS.Hosted.VLCommon;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using System.Xml;
using System.Xml.Linq;

namespace VSS.Hosted.VLCommon
{
  public static class AssetIDChangesAPI
  {
      public static AssetIDChanges GetAssetIDChanges(DateTime bookmarkDateTime)
      {
        AssetIDChanges result = new AssetIDChanges();        
        using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          result.AssetInfo = (from al in opCtx.AssetAliasReadOnly
                               join ast in opCtx.AssetReadOnly on al.fk_AssetID equals ast.AssetID
                               where al.InsertUTC >= bookmarkDateTime
                              select new AssetInfo
                               {
                                 SerialNumber = ast.SerialNumberVIN,
                                 MakeCode = ast.fk_MakeCode,
                                 AssetName = al.Name,
                                 AssetNameUpdatedUTC = al.InsertUTC, 
                                 IBKey = al.IBKey,
                                 DealerCode = al.NetworkDealerCode ?? string.Empty,
                                 DealerAccountCode = al.DealerAccountCode ?? string.Empty,
                                 CustomerCode = al.NetworkCustomerCode ?? string.Empty,
                                 OwnerBssID = al.OwnerBSSID
                               }).ToList();                   
        }                
        return result;
      }
  }
}
