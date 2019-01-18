using System;
using System.Collections.Generic;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.Alignments.Interfaces
{
  public interface IAlignments : IList<IAlignment>, IBinaryReaderWriter
  {
    void Assign(IAlignments alignments);

    /// <summary>
    /// Create a new Alignment in the list based on the provided details
    /// </summary>
    /// <param name="alignmentUid"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="extents"></param>
    /// <returns></returns>
    IAlignment AddAlignmentDetails(Guid alignmentUid,
      DesignDescriptor designDescriptor,
      BoundingWorldExtent3D extents);

    /// <summary>
    /// Remove a given Alignment from the list of Alignments for a site model
    /// </summary>
    /// <param name="alignmentUid"></param>
    /// <returns></returns>
    bool RemoveAlignment(Guid alignmentUid);

    /// <summary>
    /// Locates a Alignment in the list with the given GUID
    /// </summary>
    IAlignment Locate(Guid alignmentUid);
  }
}
