using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.AssetService.AcceptanceTests.Models
{

  public class AssetStatus
  {
    public Guid AssetUID { get; set; }
    public string Status { get; set; }
    public DateTime LastStatusUpdateUTC { get; set; }
  }
}
