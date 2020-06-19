using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface IRebuildSiteModelMetaData
  {
    /// <summary>
    /// The current phase of project rebuilding this project is in
    /// </summary>
    RebuildSiteModelPhase Phase { get; set; }

    /// <summary>
    /// The UTC date at which the last update to this metadata was made
    /// </summary>
    long LastUpdateUtcTicks { get; set; }

    /// <summary>
    /// Project being rebuilt
    /// </summary>
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// The last known submitted TAG file
    /// </summary>
    public string LastSubmittedTagFile { get; set; }

    /// <summary>
    /// The last known processed TAG file
    /// </summary>
    public string LastProcessedTagFile { get; set; }
  }
}
