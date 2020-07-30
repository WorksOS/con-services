using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Gateway.Common.Requests
{
  public class AlignmentDesignGeometryRequest : DesignDataRequest
  {
    public bool ConvertArcsToChords;
    public double ArcChordTolerance;

    public AlignmentDesignGeometryRequest(Guid projectUid, Guid designUid, string fileName = "", bool convertArcsToChords = false, double arcChordTolerance = 0.1): base(projectUid, designUid, fileName)
    {
      ConvertArcsToChords = convertArcsToChords;
      ArcChordTolerance = arcChordTolerance;
    }
  }
}
