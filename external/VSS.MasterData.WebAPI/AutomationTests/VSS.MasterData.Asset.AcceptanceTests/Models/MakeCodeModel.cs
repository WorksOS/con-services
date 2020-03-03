using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Models
{
  public class CreateMakeCOde
  {
    public Make CreateMakeEvent = new Make();
  }

  public class Make
  {
    public string MakeCode { get; set; }
    public string MakeDesc { get; set; }
    public Guid MakeUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
