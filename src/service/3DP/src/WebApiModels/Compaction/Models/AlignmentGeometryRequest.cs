using System;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Request parameters for Raptor/TRex to get center line alignment geometry an alignment file
  /// </summary>
  public class AlignmentGeometryRequest : ProjectID
  {
    public Guid? DesignUid { get; private set; }

    public string FileName { get; private set; }

    public bool ConvertArcsToChords { get; private set; }

    public double ArcChordTolerance { get; private set; }

    public AlignmentGeometryRequest(Guid projectUid, bool convertArcsToChords, double arcChordTolerance, string fileName = "", Guid? designUid = null)
    {
      ProjectUid = projectUid;
      DesignUid = designUid;
      FileName = fileName;
      ConvertArcsToChords = convertArcsToChords;
      ArcChordTolerance = arcChordTolerance;
    }
  }
}
