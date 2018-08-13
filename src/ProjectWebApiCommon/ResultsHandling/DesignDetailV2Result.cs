using System;

namespace VSS.MasterData.Project.WebAPI.Common.ResultsHandling
{
  public class DesignDetailV2Result
  {
    /// <summary>
    /// The id for the design.
    /// </summary>
    public long id { get; set; }
    /// <summary>
    /// The name for the design.
    /// </summary>
    public string name { get; set; }
    /// <summary>
    ///  FileType  Linework=0, DesignSurface=1, SurveyedSurface=2, Alignment=3, MobileLinework=4, MassHaulPlan=7
    /// </summary>
    public int fileType { get; set; }
    /// <summary>
    /// UTC DateTime file added
    /// </summary>
    public DateTime insertUTC { get; set; }
  }
}
