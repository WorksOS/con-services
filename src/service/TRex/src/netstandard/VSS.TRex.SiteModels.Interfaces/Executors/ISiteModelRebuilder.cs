using System;
using System.Threading.Tasks;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels.Interfaces.Executors
{
  public interface ISiteModelRebuilder
  {
    /// <summary>
    /// Project ID of this project this rebuilder is managing
    /// </summary>
    Guid ProjectUid { get; }

    /// <summary>
    /// Metadata for this project rebuild operation
    /// </summary>
    IRebuildSiteModelMetaData Metadata { get; }

    /// <summary>
    /// The storage proxy cache for the rebuilder to use for tracking metadata
    /// </summary>
    IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData> MetadataCache { get; set; }

    /// <summary>
    /// The storage proxy cache for the rebuilder to use to store names of TAG files requested from S3
    /// </summary>
    IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> FilesCache { get; set; }

    /// <summary>
    /// Ensures there is no current rebuilding activity for this project.
    /// The exception is when there exists a metadata record for the project and the phase is complete
    /// </summary>
    bool ValidateNoActiveRebuilderForProject(Guid projectUid);

    /// <summary>
    /// Coordinate rebuilding of a project, returning a Tasl for the caller to manage.
    /// </summary>
    Task<IRebuildSiteModelMetaData> ExecuteAsync();

    /// <summary>
    /// Notifies the rebuilder that a TAG file marked with notify after processing has been processed
    /// </summary>
    void TAGFilesProcessed(IProcessTAGFileResponseItem[] responseItems);
  }
}
