using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Defines and manages the activites executed asynchronously to rebuild a project. This class is instantiated
  /// by the project rebuilder manager for each project sbeing rebuilt
  /// </summary>
  public class SiteModelRebuilder
  {
    private static ILogger _log = Logging.Logger.CreateLogger<SiteModelRebuilder>();

    /// <summary>
    /// The storage proxy cache for the rebuilder to use
    /// </summary>
    private IStorageProxyCache<Guid, IRebuildSiteModelMetaData> _storageProxyCache
      = DIContext.Obtain<Func<IStorageProxyCache<Guid, IRebuildSiteModelMetaData>>>()();

    /// <summary>
    /// Project ID of this project this rebuilder is managing
    /// </summary>
    public Guid ProjectUid { get; }

    /// <summary>
    /// The response tht will be provided to the caller when complete
    /// </summary>
    private RebuildSiteModelRequestResponse _response;

    public SiteModelRebuilder(Guid projectUid)
    {
      ProjectUid = projectUid;

      _response = new RebuildSiteModelRequestResponse(projectUid);
    }

    /// <summary>
    /// Ensures there is no current rebuilding activity for this projecy.
    /// The exception is when there exists a metadata record for the project and the phase is complete
    /// </summary>
    public bool ValidateNoAciveRebuilderForProject()
    {
      return false;
    }

    private RebuildSiteModelRequestResponse Execute()
    {
      _log.LogInformation($"Starting rebuilding project {ProjectUid}");

      return null;
    }

    /// <summary>
    /// Coordinate rebuilding of a project, returning a Tasl for the caller to manahgw.
    /// </summary>
    public Task<RebuildSiteModelRequestResponse> ExecuteAsync()
    {
      return Task.Run(() => Execute());
    }
  }
}
