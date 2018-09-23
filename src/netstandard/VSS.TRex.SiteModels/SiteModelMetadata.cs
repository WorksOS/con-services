using System;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  public class SiteModelMetadata : ISiteModelMetadata
  {
    public Guid ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public BoundingWorldExtent3D SiteModelExtent { get; set; }
    public int MachineCount { get; set; }
    public int DesignCount { get; set; }
    public int SurveyedSurfaceCount { get; set; }
  }
}
