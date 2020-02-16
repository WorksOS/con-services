using System;
using System.Collections.Generic;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.Site
{
  [Serializable]
  public class PolygonSiteConfiguration : Block
  {
    public PolygonSiteConfiguration()
    {
      Polygon = new List<Point>();
    }

    public Guid SiteID { get; set; }
    public string Name { get; set; }
    public List<Point> Polygon { get; set; }
  }
}
