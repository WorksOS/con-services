using System;
using VSS.AWS.TransferProxy;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.SiteModels.Interfaces.Requests;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface IRebuildSiteModelMetaData : IProjectAffinity
  {
    /// <summary>
    /// The current phase of project rebuilding this project is in
    /// </summary>
    RebuildSiteModelPhase Phase { get; set; }

    /// <summary>
    /// A set of flags governing aspects of site model rebuilding, such as archival of TAG files processed during the rebuild
    /// </summary>
    RebuildSiteModelFlags Flags { get; set; }

    /// <summary>
    /// The UTC date at which the last update to this metadata was made
    /// </summary>
    long LastUpdateUtcTicks { get; set; }

    /// <summary>
    /// Defines how selective the site model delete operation should be.
    /// Selectivity allows certain portions of a site model to be deleted to help with operations
    /// like rebuilding projects on demand.
    /// </summary>
    public DeleteSiteModelSelectivity DeletionSelectivity { get; set; }

    /// <summary>
    /// The result of the deletion stage of the project rebuild
    /// </summary>
    public DeleteSiteModelResult DeletionResult { get; set; }

    /// <summary>
    /// The result of this rebuild request. As this process may be long, this response will chiefly indicate the
    /// success or failure of starting the overall process of rebuilding a project.
    /// </summary>
    public RebuildSiteModelResult RebuildResult { get; set; }

    /// <summary>
    /// Denotes the type of S3 transfer proxy the site model rebuilder should use to scan and source TAG files for reprocessing
    /// This allows different sources such as the primary TAG file archive, or a prepared project migration bucket to be referenced
    /// </summary>
    public TransferProxyType OriginS3TransferProxy { get; set; }

    /// <summary>
    /// The number of tag files extracted from the S3 repository ready to submit for processing
    /// </summary>
    public int NumberOfTAGFilesFromS3 { get; set; }

    /// <summary>
    /// The number of collections of tag file keys extracted from the S3 repository have been submitted into the file cache
    /// </summary>
    public int NumberOfTAGFileKeyCollections { get; set; }

    /// <summary>
    /// The last known submitted TAG file
    /// </summary>
    public string LastSubmittedTagFile { get; set; }

    /// <summary>
    /// The last known processed TAG file
    /// </summary>
    public string LastProcessedTagFile { get; set; }

    /// <summary>
    /// The number of TAG files submitted to the TAG file processor
    /// </summary>
    public int NumberOfTAGFilesSubmitted { get; set; }

    /// <summary>
    /// The number of TAG file reported processed by the TAG file processor
    /// </summary>
    public int NumberOfTAGFilesProcessed { get; set; }
  }
}
