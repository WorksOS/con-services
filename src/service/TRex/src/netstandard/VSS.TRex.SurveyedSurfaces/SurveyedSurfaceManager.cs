using Microsoft.Extensions.Logging;
using System;
using System.IO;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SurveyedSurfaces
{
  /// <summary>
  /// The surveyed surface manager responsible for orchestrating access and mutations against the surveyed surfaces held for a project.
  /// </summary>
  public class SurveyedSurfaceManager : ISurveyedSurfaceManager
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SurveyedSurfaceManager>();

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
    /// <param name="siteModelUid"></param>
    /// <returns></returns>
    private ISurveyedSurfaces Load(Guid siteModelUid)
    {
      _readStorageProxy.ReadStreamFromPersistentStore(siteModelUid, SURVEYED_SURFACE_STREAM_NAME, FileSystemStreamType.SurveyedSurfaces, out MemoryStream ms);

      var ss = DIContext.Obtain<ISurveyedSurfaces>();

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
    /// <param name="siteModelUid"></param>
    /// <param name="surveyedSurfaces"></param>
    private void Store(Guid siteModelUid, ISurveyedSurfaces surveyedSurfaces)
    {
      using (var stream = surveyedSurfaces.ToStream())
      {
        if (_writeStorageProxy.WriteStreamToPersistentStore(siteModelUid, SURVEYED_SURFACE_STREAM_NAME,
          FileSystemStreamType.SurveyedSurfaces, stream, this) == FileSystemErrorStatus.OK)
        {
          _writeStorageProxy.Commit();
        }
      }

      // Notify the  grid listeners that attributes of this site model have changed.
      var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
      sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyAll, siteModelUid, surveyedSurfacesChanged: true);
    }

    /// <summary>
    /// Add a new surveyed surface to a site model
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="asAtDate"></param>
    /// <param name="extents"></param>
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
      Log.LogInformation($"Listing surveyed surfaces from site model {siteModelUid}");

      return Load(siteModelUid);
    }

    /// <summary>
    /// Remove a given surveyed surface from a site model
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="surveySurfaceUid"></param>
    /// <returns></returns>
    public bool Remove(Guid siteModelUid, Guid surveySurfaceUid)
    {
      var ss = Load(siteModelUid);
      bool result = ss.RemoveSurveyedSurface(surveySurfaceUid);
      Store(siteModelUid, ss);

      return result;
    }
  }
}
