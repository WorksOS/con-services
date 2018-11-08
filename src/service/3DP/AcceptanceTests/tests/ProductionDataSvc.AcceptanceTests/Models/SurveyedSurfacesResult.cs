using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class GetSurveydSurfacesResult : ResponseBase
  {
    #region Members
    public List<SurveyedSurfaces> SurveyedSurfaces { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor: success result by default
    /// </summary>
    public GetSurveydSurfacesResult()
        : base("success")
    { }
    #endregion
  }
}
