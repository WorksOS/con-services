using System;
using System.Collections.Generic;
using VSS.TRex.Geometry;

namespace VSS.TRex.TAGFiles.Executors
{
  public interface IACSTranslator
  {
    public List<UTMCoordPointPair> TranslatePositions(Guid? targetProjectUid, List<UTMCoordPointPair> coordPositions);
  }
}
