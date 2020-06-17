using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces
{
  /// <summary>
  /// The surveyed surface manager responsible for orchestrating access and mutations against the surveyed surfaces held for a project.
  /// </summary>
  public class SurveyedSurfaceManager : ISurveyedSurfaceManager
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SurveyedSurfaceManager>();

    private readonly IStorageProxy _writeStorageProxy;
    private readonly IStorageProxy _readStorageProxy;

    public const string SURVEYED_SURFACE_STREAM_NAME = "SurveyedSurfaces";

    /// <summary>
    /// Constructs an instance using the supplied storage proxy
    /// </summary>
    public SurveyedSurfaceManager(StorageMutability mutability)
    {
      _writeStorageProxy = DIContext.Obtain<ISiteModels>().PrimaryMutableStorageProxy;
      _readStorageProxy = DIContext.Obtain<ISiteModels>().PrimaryStorageProxy(mutability);
    }

    /// <summary>
    /// Loads the set of surveyed surfaces for a site model. If none exist and empty list is returned.
    /// </summary>
    private ISurveyedSurfaces Load(Guid siteModelUid)
    {
      var ss = DIContext.Obtain<ISurveyedSurfaces>();

      if (ss == null)
      {
        _log.LogError("Unable to access surveyed surfaces factory from DI");
        return null;
      }

      var task = _readStorageProxy.ReadStreamFromPersistentStore(siteModelUid, SURVEYED_SURFACE_STREAM_NAME, FileSystemStreamType.SurveyedSurfaces);

      task.Wait(); // TODO: Move higher later
      var ms = task.Result.Item2;

      if (ms != null)
      {
        using (ms)
        {
          ss.FromStream(ms);
        }
      }

      return ss;
    }

    /// <summary>
    /// Stores the list of surveyed surfaces for a site model
    /// </summary>
    private async Task Store(Guid siteModelUid, ISurveyedSurfaces surveyedSurfaces)
    {
      await using var stream = surveyedSurfaces.ToStream();
      if (await _writeStorageProxy.WriteStreamToPersistentStore(siteModelUid, SURVEYED_SURFACE_STREAM_NAME,
        FileSystemStreamType.SurveyedSurfaces, stream, this) == FileSystemErrorStatus.OK)
      {
        _writeStorageProxy.Commit();
      }

      // Notify the grid listeners that attributes of this site model have changed.
      var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
      sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyAll, siteModelUid, surveyedSurfacesChanged: true);
    }

    /// <summary>
    /// Add a new surveyed surface to a site model
    /// </summary>
    public ISurveyedSurface Add(Guid siteModelUid, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents)
    {
      if (asAtDate.Kind != DateTimeKind.Utc)
        throw new ArgumentException("AsAtDate must be a UTC date time");

      var ss = Load(siteModelUid);
      var newSurveyedSurface = ss.AddSurveyedSurfaceDetails(designDescriptor.DesignID, designDescriptor, asAtDate, extents);
      Store(siteModelUid, ss);

      return newSurveyedSurface;
    }

    /// <summary>
    /// List the surveyed surfaces for a site model
    /// </summary>
    public ISurveyedSurfaces List(Guid siteModelUid)
    {
      _log.LogInformation($"Listing surveyed surfaces from site model {siteModelUid}");

      return Load(siteModelUid);
    }

    /// <summary>
    /// Remove a given surveyed surface from a site model
    /// </summary>
    public bool Remove(Guid siteModelUid, Guid surveySurfaceUid)
    {
      var ss = Load(siteModelUid);
      var result = ss.RemoveSurveyedSurface(surveySurfaceUid);
      Store(siteModelUid, ss);

      return result;
    }

    /// <summary>
    /// Remove the surveyed surface list for a site model from the persistent store
    /// </summary>
    public bool Remove(Guid siteModelUid, IStorageProxy storageProxy)
    {
      var result = storageProxy.RemoveStreamFromPersistentStore(siteModelUid, FileSystemStreamType.Designs, SURVEYED_SURFACE_STREAM_NAME);

      if (result != FileSystemErrorStatus.OK)
      {
        _log.LogInformation($"Removing surveyed surfaces list from project {siteModelUid} failed with error {result}");
      }

      return result == FileSystemErrorStatus.OK;
    }
  }
}
