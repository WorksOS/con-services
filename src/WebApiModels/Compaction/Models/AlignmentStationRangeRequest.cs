using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  public class AlignmentStationRangeRequest : ProjectID, IValidatable
  {
    public DesignDescriptor fileDescriptor { get; set; }
  }
}