using System.Collections.Generic;

namespace CCSS.WorksOS.Reports.Common.DataGrabbers
{
  public class JModel
  {
    public Dictionary<string, string> Headers { get; set; }
    public List<Dictionary<string, string>> Data { get; set; }
    public Dictionary<string, string> Summary { get; set; }
    public Metadata Metadata { get; set; }

  }
}
