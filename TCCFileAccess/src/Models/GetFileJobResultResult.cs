using System.Collections.Generic;

namespace VSS.TCCFileAccess.Models
{
  //Note: This does NOT derive from ApiResult
  public class GetFileJobResultResult
  {
    public Extents extents;
    public Projection projection;
  }

  public class Extents
  {
    public double latitude1;
    public double latitude2;
    public double longitude1;
    public double longitude2;
  }
  public class Projection
  {
    public int projection;
    public int datum;
    public int unit;
    public int zone;
    public List<object> attributes;
  }
}
