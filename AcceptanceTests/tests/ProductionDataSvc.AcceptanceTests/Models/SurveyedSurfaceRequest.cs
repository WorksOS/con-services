using System;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The request representation for managing of the Raptor’s list of ground/surveyed SurveyedSurfaces.
  /// </summary>
  /// 
  public class SurveyedSurfaceRequest : RequestBase
  {
    /// <summary>
    /// Project ID. Required.
    /// </summary>
    ///
    public long projectID { get; set; }

    /// <summary>
    /// Description to identify a surveyed surface file either by id or by its location in TCC.
    /// </summary>
    /// 
    public DesignDescriptor SurveyedSurface { get; set; }

    /// <summary>
    /// Surveyed UTC date/time.
    /// </summary>
    /// 
    public DateTime surveyedUTC { get; set; }
  }
}
