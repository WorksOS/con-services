﻿using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SiteModels.Interfaces;

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
    private readonly Dictionary<Guid, IDesignBase> designs = new Dictionary<Guid, IDesignBase>();

    /// <summary>
    /// Removes a design from cache and storage
    /// </summary>
    public bool RemoveDesignFromCache(Guid designUid, IDesignBase design, Guid siteModelUid, bool deleteFile)
    {
      if (deleteFile)
        design.RemoveFromStorage(siteModelUid, Path.GetFileName(design.FileName));

      if (designs.TryGetValue(designUid, out _))
      {
        return designs.Remove(designUid);
      }

      return false;
    }

    /// <summary>
    /// Acquire a lock and reference to the design referenced by the given design descriptor
    /// </summary>
    /// <param name="designUid"></param>
    /// <param name="dataModelID"></param>
    /// <param name="cellSize"></param>
    /// <param name="loadResult"></param>
    /// <returns></returns>
    public IDesignBase Lock(Guid designUid, Guid dataModelID, double cellSize, out DesignLoadResult loadResult)
    {
      IDesignBase design;

      lock (designs)
      {
        designs.TryGetValue(designUid, out design);

        if (design == null)
        {
          // Verify the design does exist in either the designs or surveyed surface lists for the site model
          var designRef = DIContext.Obtain<ISiteModels>().GetSiteModel(dataModelID).Designs.Locate(designUid);
          var descriptor = designRef?.DesignDescriptor;

          if (descriptor == null)
          {
            var surveyedSurfaceRef = DIContext.Obtain<ISiteModels>().GetSiteModel(dataModelID).SurveyedSurfaces?.Locate(designUid);
            descriptor = surveyedSurfaceRef?.DesignDescriptor;
          }
          
          if (descriptor == null)
          {
            loadResult = DesignLoadResult.DesignDoesNotExist;
            return null;
          }

          // Add a design in the 'IsLoading state' to control multiple access to this design until it is fully loaded
          design = DIContext.Obtain<IDesignClassFactory>().NewInstance(Path.Combine(FilePathHelper.GetTempFolderForProject(dataModelID), descriptor.FileName), cellSize, dataModelID);
          design.IsLoading = true;

          designs.Add(designUid, design);
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
          // TODO we need to take away this async code from the lock
          loadResult = design.LoadFromStorage(dataModelID, Path.GetFileName(design.FileName), Path.GetDirectoryName(design.FileName), true).Result;
          if (loadResult != DesignLoadResult.Success)
          {
            designs.Remove(designUid);
            return null;
          }
        }

        loadResult = design.LoadFromFile(design.FileName);
        if (loadResult != DesignLoadResult.Success)
        {
          designs.Remove(designUid);
          return null;
        }

        design.IsLoading = false;
        return design;
      }
    }

    /// <summary>
    /// Release a lock to the design referenced by the given design descriptor
    /// </summary>
    /// <param name="designUid"></param>
    /// <param name="design"></param>
    /// <returns></returns>
    public bool UnLock(Guid designUid, IDesignBase design)
    {
      lock (designs)
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
