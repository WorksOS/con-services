using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using Microsoft.Extensions.Logging;
using VSS.TRex.SiteModels.Interfaces;
using Nito.AsyncEx.Synchronous;

namespace VSS.TRex.Designs
{

  /*
TDesignFiles = class(TObject)
  private
    FDesignCacheSizeInKB : Integer;
    FCurrentCacheSizeInKB : Integer;
    FDesignUnlockedEvent : TSimpleEvent;
    function EnsureSufficientSpaceToLoadDesign(SpaceRequiredInKB: Integer): Boolean;
    function SpaceAvailableInKB: Integer;
    function ImportFileFromTCC(const DesignDescriptor : TVLPDDesignDescriptor; const ProjectUid : Int64) : Boolean;

    procedure DeleteLocallyCachedFile(const FileToDelete: TFileName);

  public
    Function AnyLocks(out LockCount : integer) : Boolean;

    function GetCombinedSubgridIndexStream(const Surfaces: TICGroundSurfaceDetailsList;
                                           const ProjectUid : Int64; const ACellSize: Double;
                                           out MS: TMemoryStream): Boolean;

    procedure UpdateDesignCache(const ProjectUid     :Int64;
                                const DesignFileName  :String;
                                const DeleteTTMFile   :Boolean);
end;
*/

  public class DesignFiles : IDesignFiles
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<DesignFiles>();

    private readonly Dictionary<Guid, IDesignBase> _designs = new Dictionary<Guid, IDesignBase>();

    /// <summary>
    /// Removes a design from cache and storage
    /// </summary>
    public bool RemoveDesignFromCache(Guid designUid, IDesignBase design, Guid siteModelUid, bool deleteFile)
    {
      _log.LogDebug($"Removing design UID {designUid}, filename = '{design.FileName}'");

      if (deleteFile)
        design.RemoveFromStorage(siteModelUid, Path.GetFileName(design.FileName));

      if (_designs.TryGetValue(designUid, out _))
      {
        return _designs.Remove(designUid);
      }

      return false;
    }

    /// <summary>
    /// Acquire a lock and reference to the design referenced by the given design descriptor
    /// </summary>
    public IDesignBase Lock(Guid designUid, Guid dataModelId, double cellSize, out DesignLoadResult loadResult)
    {
      IDesignBase design;

      lock (_designs)
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(dataModelId);
        if (siteModel == null)
        {
          _log.LogWarning($"Failed to get site model with ID {dataModelId} for design {designUid}");
          loadResult = DesignLoadResult.SiteModelNotFound;
          return null;
        }

        _designs.TryGetValue(designUid, out design);

        if (design == null)
        {
          _log.LogDebug($"Design UID {designUid} not present in cached designs for site model {dataModelId}");

          // Verify the design does exist in either the designs, surveyed surface or alignment lists for the site model
          var designRef = siteModel.Designs.Locate(designUid);
          var descriptor = designRef?.DesignDescriptor;

          if (descriptor != null)
          {
            _log.LogDebug($"Surface design UID {designUid}, filename = {descriptor.FileName} needs to be loaded");
          }

          if (descriptor == null)
          {
            var surveyedSurfaceRef = siteModel.SurveyedSurfaces?.Locate(designUid);
            descriptor = surveyedSurfaceRef?.DesignDescriptor;
          }

          if (descriptor != null)
          {
            _log.LogDebug($"Surveyed surface design UID {designUid}, filename = {descriptor.FileName} needs to be loaded");
          }

          if (descriptor == null)
          {
            var alignmentDesignRef = siteModel.Alignments?.Locate(designUid);
            descriptor = alignmentDesignRef?.DesignDescriptor;
          }

          if (descriptor != null)
          {
            _log.LogDebug($"Alignment design UID {designUid}, filename = {descriptor.FileName} needs to be loaded");
          }

          if (descriptor == null)
          {
            _log.LogWarning($"Failed to locate design {designUid} for site model with ID {dataModelId}");
            loadResult = DesignLoadResult.DesignDoesNotExist;
            return null;
          }

          _log.LogDebug($"Creating entry for design UID {designUid}, filename = {descriptor.FileName} within the in-memory cache");

          // Add a design in the 'IsLoading state' to control multiple access to this design until it is fully loaded
          design = DIContext.Obtain<IDesignClassFactory>().NewInstance(Path.Combine(FilePathHelper.GetTempFolderForProject(dataModelId), descriptor.FileName), cellSize, dataModelId);
          design.IsLoading = true;

          _designs.Add(designUid, design);
        }
      }

      lock (design)
      {
        if (!design.IsLoading)
        {
          loadResult = DesignLoadResult.Success;
          return design;
        }

        if (!File.Exists(design.FileName))
        {
          _log.LogDebug($"Loading design UID {designUid}, filename = {design.FileName}");

          loadResult = design.LoadFromStorage(dataModelId, Path.GetFileName(design.FileName), Path.GetDirectoryName(design.FileName), true).WaitAndUnwrapException();
          if (loadResult != DesignLoadResult.Success)
          {
            _log.LogWarning($"Failed to load design {designUid} from file {design.FileName}, from persistent storage for site model with ID {dataModelId}");
            _designs.Remove(designUid);
            return null;
          }
        }

        loadResult = design.LoadFromFile(design.FileName);
        if (loadResult != DesignLoadResult.Success)
        {
          _log.LogWarning($"Failed to load design {designUid} from file {design.FileName}, from local storage for site model with ID {dataModelId}");
          _designs.Remove(designUid);
          return null;
        }

        design.IsLoading = false;
        return design;
      }
    }

    /// <summary>
    /// Release a lock to the design referenced by the given design descriptor
    /// </summary>
    public bool UnLock(Guid designUid, IDesignBase design)
    {
      lock (_designs)
      {
        // Very simple unlock function...
        return true;
      }
    }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public DesignFiles()
    {
    }
  }
}
