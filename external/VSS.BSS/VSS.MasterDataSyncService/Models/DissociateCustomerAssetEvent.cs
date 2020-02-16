﻿using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class DissociateCustomerAssetEvent:ICustomerAssetEvent
  {
    public Guid CustomerUID { get; set; }

    public Guid AssetUID { get; set; }

    public DateTime ActionUTC { get; set; }
  }
}
