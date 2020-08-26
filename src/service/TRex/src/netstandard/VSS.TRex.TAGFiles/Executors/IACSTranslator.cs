using System.Collections.Generic;
using VSS.TRex.Geometry;

namespace VSS.TRex.TAGFiles.Executors
{
  public interface IACSTranslator
  {
    public List<UTMCoordPointPair> TranslatePositions(string projectCSIBFile, List<UTMCoordPointPair> coordPositions);
  }
}
