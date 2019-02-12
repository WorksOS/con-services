using System.Collections.Generic;
using VSS.Productivity3D.Filter.Abstractions.Models;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class FilterListRequest
  {
    public IEnumerable<FilterRequest> FilterRequests { get; set; }
  }
}
