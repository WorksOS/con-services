using System;
using System.Collections.Generic;
using System.Text;
using VSS.Productivity3D.Filter.Common.Models;

namespace TestUtility.Model.WebApi
{
  public class FilterListRequest
  {
    public List<FilterRequest> FilterRequests { get; set; }

    public FilterListRequest()
    {
      FilterRequests = new List<FilterRequest>();
    }
  }
}
