using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Project.Data.Models
{
  public class CustomerProject
  {
    public string ProjectUID { get; set; }
    public string CustomerUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}
