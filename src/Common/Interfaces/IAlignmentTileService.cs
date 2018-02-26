using System.Collections.Generic;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.MapHandling;

namespace VSS.Productivity3D.Common.Interfaces
{
  public interface IAlignmentTileService
  {
    byte[] GetAlignmentsBitmap(MapParameters parameters, long projectId, IEnumerable<DesignDescriptor> alignmentDescriptors);
    (double? startOffset, double? endOffset) GetAlignmentOffsets(long projectId, DesignDescriptor alignDescriptor);
  }
}