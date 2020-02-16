using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.Site
{
  [Serializable]
  public class RemovePolygonSite : Block
  {
    public Guid SiteID { get; set; }
  }
}
