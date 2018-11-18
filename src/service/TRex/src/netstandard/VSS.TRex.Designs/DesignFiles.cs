using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;

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
      function ImportFileFromTCC(const DesignDescriptor : TVLPDDesignDescriptor; const DataModelID : Int64) : Boolean;

      procedure DeleteLocallyCachedFile(const FileToDelete: TFileName);

    public
      Function AnyLocks(out LockCount : integer) : Boolean;

      function GetCombinedSubgridIndexStream(const Surfaces: TICGroundSurfaceDetailsList;
                                             const DataModelID : Int64; const ACellSize: Double;
                                             out MS: TMemoryStream): Boolean;

      procedure UpdateDesignCache(const DataModelID     :Int64;
                                  const DesignFileName  :String;
                                  const DeleteTTMFile   :Boolean);
  end;
  */

  public class DesignFiles : IDesignFiles
  {
        private readonly Dictionary<Guid, IDesignBase> designs = new Dictionary<Guid, IDesignBase>();

        public bool RemoveDesignFromCache(Guid designUid, IDesignBase design, bool deleteFile)
        {
            if (deleteFile)
            {
                Debug.Assert(false, "Delete file not implemented");
                return false;
            }

            if (designs.TryGetValue(designUid, out _))
            {
                return designs.Remove(designUid);
            }

            return false;
        }

        public void AddDesignToCache(Guid designUid, IDesignBase design)
        {
            lock (designs)
            {
                if (designs.TryGetValue(designUid, out _))
                {
                    // The design is already there...
                    Debug.Assert(false, $"Error adding design {designUid} to designs, already present.");
                    return;
                }

                designs.Add(designUid, design);
            }
        }

        /// <summary>
        /// Acquire a lock and reference to the design referenced by the given design descriptor
        /// </summary>
        /// <param name="designUid"></param>
        /// <param name="DataModelID"></param>
        /// <param name="ACellSize"></param>
        /// <param name="LoadResult"></param>
        /// <returns></returns>
        public IDesignBase Lock(Guid designUid,
                               Guid DataModelID, double ACellSize, out DesignLoadResult LoadResult)
        {
            IDesignBase design;

            // Very simple lock function...
            lock (designs)
            {
                designs.TryGetValue(designUid, out design);
            }

            if (design == null)
            {
              // Load the design into the cache (in this case just TTM files)
              design = new TTMDesign(ACellSize);
              if (!File.Exists(design.FileName))
              {
                design.LoadFromStorage(DataModelID, Path.GetFileName(design.FileName), design.FileName, true);
              }

                design.LoadFromFile(design.FileName);

                AddDesignToCache(designUid, design);
            }

            LoadResult = DesignLoadResult.Success;
            return design;
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
