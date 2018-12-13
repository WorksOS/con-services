using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;

namespace VSS.TRex.SurveyedSurfaces
{
  /// <summary>
  /// The surveyed surface manager responsible for orchestrating access and mutations against the surveyed surfaces held for a project.
  /// </summary>
  public class SurveyedSurfaceManager : ISurveyedSurfaceManager
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SurveyedSurfaceManager>();

    private readonly IStorageProxy WriteStorageProxy;
    private readonly IStorageProxy ReadStorageProxy;

    private const string SURVEYED_SURFACE_STREAM_NAME = "SurveyedSurfaces";

    /// <summary>
    /// Constructs an instance using the supplied storage proxy
    /// </summary>
    public SurveyedSurfaceManager()
    {
      WriteStorageProxy = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();
      ReadStorageProxy = DIContext.Obtain<ISiteModels>().StorageProxy;
    }

    /// <summary>
    /// Loads the set of surveyed surfaces for a sitemodel. If none exist and empty list is returned.
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <returns></returns>
    private ISurveyedSurfaces Load(Guid siteModelID)
    {
      try
      {
        ReadStorageProxy.ReadStreamFromPersistentStore(siteModelID, SURVEYED_SURFACE_STREAM_NAME, FileSystemStreamType.SurveyedSurfaces, out MemoryStream ms);

        ISurveyedSurfaces ss = DIContext.Obtain<ISurveyedSurfaces>();

        if (ms != null)
        { 
          using (ms)
          {
            ss.FromStream(ms);
          }
        }

        return ss;
      }
      catch (KeyNotFoundException)
      {
        /* This is OK, the element is not present in the cache yet */
      }
      catch (Exception e)
      {
        throw new TRexException("Exception reading surveyed surfaces cache element from Ignite", e);
      }

      return null;
    }

    /// <summary>
    /// Stores the list of surveyed surfaces for a site model
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="ss"></param>
    private void Store(Guid siteModelID, ISurveyedSurfaces ss)
    {
      try
      {
        WriteStorageProxy.WriteStreamToPersistentStore(siteModelID, SURVEYED_SURFACE_STREAM_NAME, FileSystemStreamType.SurveyedSurfaces, ss.ToStream(), this);
        WriteStorageProxy.Commit();

        // Notify the  grid listeners that attributes of this sitemodel have changed.
        var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
        sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyImmutable, siteModelID, surveyedSurfacesChanged: true);
      }
      catch (Exception e)
      {
        throw new TRexException("Exception writing updated surveyed surfaces cache element to Ignite", e);
      }
    }
    
    /// <summary>
    /// Add a new surveyed surface to a sitemodel
    /// </summary>
    /// <param name="SiteModelID"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="asAtDate"></param>
    /// <param name="extents"></param>
    public ISurveyedSurface Add(Guid SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents)
    {
      ISurveyedSurfaces ss = Load(SiteModelID);
      ISurveyedSurface newSurveyedSurface = ss.AddSurveyedSurfaceDetails(Guid.NewGuid(), designDescriptor, asAtDate, extents);
      Store(SiteModelID, ss);

      return newSurveyedSurface;
    }

    /// <summary>
    /// List the surveyed surfaces for a site model
    /// </summary>
    public ISurveyedSurfaces List(Guid SiteModelID)
    {
      Log.LogInformation($"Listing surveyed surfaces from site model {SiteModelID}");

      return Load(SiteModelID);
    }

    /// <summary>
    /// Remove a given surveyed surface from a site model
    /// </summary>
    /// <param name="SiteModelID"></param>
    /// <param name="SurveySurfaceID"></param>
    /// <returns></returns>
    public bool Remove(Guid SiteModelID, Guid SurveySurfaceID)
    {
      ISurveyedSurfaces ss = Load(SiteModelID);
      bool result = ss.RemoveSurveyedSurface(SurveySurfaceID);
      Store(SiteModelID, ss);

      return result;
    }
  }
}
