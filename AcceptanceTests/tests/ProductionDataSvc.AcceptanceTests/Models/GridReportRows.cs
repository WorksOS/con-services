using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class GridReportRows
  {
    public double Northing { get; set; }
    public double Easting { get; set; }
    public double Elevation { get; set; }
    public double CutFill { get; set; }
    public double CMV { get; set; }
    public double MDP { get; set; }
    public double PassCount { get; set; }
    public double Temperature { get; set; }
  }
}
