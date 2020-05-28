using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Gateway.Common.Requests
{
  public class AlignmentDesignGeometryRequest : DesignDataRequest
  {
    public AlignmentDesignGeometryRequest(Guid projectUid, Guid designUid, string fileName = "") : base(projectUid, designUid, fileName)
    { 
    }
  }
}
