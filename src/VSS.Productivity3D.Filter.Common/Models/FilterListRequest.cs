using System.Collections.Generic;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class FilterListRequest
  {
    public IEnumerable<FilterRequest> FilterRequests { get; set; }
  }
}