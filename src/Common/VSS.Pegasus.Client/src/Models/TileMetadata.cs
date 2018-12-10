using System.Collections.Generic;

namespace VSS.Pegasus.Client.Models
{
  public class TileMetadata
  {
    public string tilejson { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string attribution { get; set; }
    public List<string> tiles { get; set; }
    public int minzoom { get; set; }
    public int maxzoom { get; set; }
    public List<double> bounds { get; set; }
    public List<double> center { get; set; }
  }
}
