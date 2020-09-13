using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  public class AlignmentStationRangeRequest : ProjectID, IValidatable
  {
    public DesignDescriptor fileDescriptor { get; set; }
  }
}
