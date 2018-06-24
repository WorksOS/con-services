using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Types
{
  /// <summary>
  /// Notes the type of surveyed surface patch result required froma surveyed surface patch request
  /// </summary>
  public enum SurveyedSurfacePatchType
  {
    /// <summary>
    /// The latest (in time) available elevation at each location from a set of surveyed surfaces
    /// </summary>
    LatestSingleElevation,

    /// <summary>
    /// The earliest (in time) available elevation at each location from a set of surveyed surfaces
    /// </summary>
    EarliestSingleElevation,

    /// <summary>
    /// THe analysed first, last, lowest and heighest elevations at each location from a set of surveyed surfaces
    /// </summary>
    CompositeElevations
  }
}
