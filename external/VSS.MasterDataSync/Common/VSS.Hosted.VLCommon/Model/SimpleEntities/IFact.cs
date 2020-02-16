using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public interface IFact
  {
    int fk_AssetKeyDate { get; set;  }
    long ifk_DimAssetID { get; set; }
  }
}
