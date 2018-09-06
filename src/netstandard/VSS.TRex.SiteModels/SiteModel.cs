using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Services.SurveyedSurfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.SiteModels
{
//  [Serializable]
    public class SiteModel : ISiteModel
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        public const string kSiteModelXMLFileName = "ProductionDataModel.XML";
        public const string kSubGridExistanceMapFileName = "SubGridExistanceMap";

        private const int kMajorVersion = 1;
        private const int kMinorVersion = 0;
        private const int kMinorVersionLatest = 0;

        public Guid ID { get; set; } = Guid.Empty;

        public DateTime LastModifiedDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// The grid data for this site model
        /// </summary>
        [NonSerialized]
        private IServerSubGridTree grid;

        /// <summary>
        /// The grid data for this site model
        /// </summary>
        public IServerSubGridTree Grid { get { return grid; } }

        [NonSerialized]
        private ISubGridTreeBitMask existanceMap;

        public ISubGridTreeBitMask ExistanceMap {  get { return existanceMap; } }

        /// <summary>
        /// SiteModelExtent records the 3D extents of the data stored in the site model
        /// </summary>
        public BoundingWorldExtent3D SiteModelExtent { get; } = BoundingWorldExtent3D.Inverted();

        /// <summary>
        /// Local cached copy of the coordiante system CSIB
        /// </summary>
        private string csib = null;

        /// <summary>
        /// The string serialized CSIB gained from adding a coordinate system from a DC or similar file
        /// to the project. This getter is reponsible for accessing the information from the persistent
        /// store and caching it in the site model
        /// </summary>
        public string CSIB()
        {
          if (csib != null)
            return csib;

          FileSystemErrorStatus readResult = DIContext.Obtain<ISiteModels>().ImmutableStorageProxy().ReadStreamFromPersistentStore(ID, CoordinateSystemConsts.kCoordinateSystemCSIBStorageKeyName, FileSystemStreamType.CoordinateSystemCSIB, out MemoryStream csibStream);

          if (readResult != FileSystemErrorStatus.OK || csibStream == null || csibStream.Length == 0)
            return null;

          using (csibStream)
          {
            return Encoding.ASCII.GetString(csibStream.ToArray());
          }
        }
  
        // ProofingRuns is the set of proofing runs that have been collected in this site model
        // public SiteProofingRuns ProofingRuns;

        // MachinesTargetValues stores a list of target values, one list per machine,
        // that record how the cofigured target CCV and pass count settings on each
        // machine has changed over time.
        [NonSerialized]
        private /*EfficientMachinesTargetValuesList*/ IMachinesProductionEventLists machinesTargetValues;
        public /*EfficientMachinesTargetValuesList*/ IMachinesProductionEventLists MachinesTargetValues
        {
          get => machinesTargetValues;
          set => machinesTargetValues = value;
        }

        private SiteModelDesignList siteModelDesigns = new SiteModelDesignList();

        /// <summary>
        /// SiteModelDesigns records all the designs that have been seen in this sitemodel.
        /// Each site model designs records the name of the site model and the extents
        /// of the cell information that have been record for it.
        /// </summary>
        public ISiteModelDesignList SiteModelDesigns { get { return siteModelDesigns; } }

        private ISurveyedSurfaces surveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

        // This is a list of TTM descriptors which indicate designs
        // that can be used as a snapshot of an actual ground surface at a specific point in time
        public ISurveyedSurfaces SurveyedSurfaces
        {
            get
            {
                if (!SurveyedSurfacesLoaded)
                {
                    SurveyedSurfaceService proxy = new SurveyedSurfaceService(StorageMutability.Immutable, TRexCaches.ImmutableNonSpatialCacheName());
                    proxy.Init(null); // Note: Not needed when this moves to Ignite deployed service model
                    ISurveyedSurfaces ss = proxy.ListDirect(ID);

                    lock (this)
                    {
                        surveyedSurfaces = ss;
                        SurveyedSurfacesLoaded = true;
                    }
                }

                return surveyedSurfaces;
            }
        }

        // FSiteModelDesignNames is an integrated list of all the design names that have appeared
        // in design change events. It shadows the FSiteModelDesigns to an alarming degree
        // and FSiteModelDesigns could either be refactored to use it, or the two could be
        // merged in intent.
        // public SiteModelDesignNames : TICClientDesignNames;

        // Machines contains a list of compactor machines that this site model knows
        // about. Each machine contains a link to the machine hardware ID for the
        // appropriate machine, and may also reference a wireless machine looked
        // after by METScomms

        public IMachinesList Machines { get; set; }

        public bool IgnoreInvalidPositions { get; set; } = true;

        private bool SurveyedSurfacesLoaded { get; set; }

        public SiteModel()
        {
            // FTransient = false
        }

        public SiteModel(Guid id) : this()
        {
            ID = id;

            // FCreationDate:= Now;
            // FMarkedForRemoval:= False;

            // FName:= Format('SiteModel-%d', [AID]);
            // FDescription:= '';
            // FActive:= True;

            Machines = new MachinesList(id);

            MachinesTargetValues = new MachinesProductionEventLists(this); //EfficientMachinesTargetValuesList(this);

            LastModifiedDate = DateTime.MinValue;

            // FSiteModelDesignNames:= TICClientDesignNames.Create(FID);

            grid = new ServerSubGridTree(ID);

            existanceMap = new SubGridTreeSubGridExistenceBitMask();

            // FProofingRuns:= TICSiteProofingRuns.Create;

            // FMaxInterEpochDist:= kMaxEpochInterval;

            // FWorkingSiteModelExtent.SetInverted;

            // FGroundSurfacesInterlock:= TCriticalSection.Create;

            // FLoadFromPersistentStoreInterlock:= TCriticalSection.Create;

            // FIsNewlyCreated:= False;

            // FAttributesChangedNotificationCount:= 0;

            // FSiteModelPersistenceSerialisationInterlock:= TCriticalSection.Create;
            // FMachineEventsPersistenceSerialisationInterlock:= TCriticalSection.Create;

            // SetLength(FTransientMachineTargetsLocks, 0);

            // {$IFDEF DATAMODEL_WRITES_SUPPORTED}
            // FPendingSubgridWritesMap:= TSubGridTreeBitMask.Create(FGrid.NumLevels - 1, FGrid.CellSize * kSubGridTreeDimension);
            // PendingSiteModelPendingPersistences:= 0;
            // {$ENDIF}
        }

        public SiteModel(//AOwner: TICSiteModels;
                         //string name,
                         //string description,
                         Guid id,
                         double cellSize) : this(id)
        {
            //        FName := AName;
            //  FDescription := ADescription;
            Grid.CellSize = cellSize;

            //  {$IFDEF DATAMODEL_WRITES_SUPPORTED}
            //FPendingSubgridWritesMap.CellSize := FGrid.CellSize* kSubGridTreeDimension;
            //  {$ENDIF}
        }

        public void Include(ISiteModel Source)
        {
            // SiteModel extents
            SiteModelExtent.Include(Source.SiteModelExtent);

            // TODO: FWorkingSiteModelExtent needed
            // FWorkingSiteModelExtent.Include(Source.WorkingSiteModelExtent);
        
            // Proofing runs
            /* TODO: Proofing runs
            for (int I = 0; I < Source.ProofingRuns.ProofingRuns.Count; I++)
              with Source.ProofingRuns.ProofingRuns[I] do
                begin
                  Index := FProofingRuns.IndexOf(Name, MachineID, StartTime, EndTime);
        
                  if Index = -1 then
                    FProofingRuns.CreateNew(Name, MachineID, StartTime, EndTime, Extents)
                  else
                    begin
                      FProofingRuns.ProofingRuns[Index].Extents.Include(Extents);
                      if FProofingRuns.ProofingRuns[Index].StartTime > StartTime then
                        FProofingRuns.ProofingRuns[Index].StartTime := StartTime;
                      if FProofingRuns.ProofingRuns[Index].EndTime<EndTime then
                        FProofingRuns.ProofingRuns[Index].EndTime := EndTime;
                    end;
                end;
             */
            // Designs
            // Note: Design names are handled as a part of integration of machine events
        
            LastModifiedDate = Source.LastModifiedDate;
        }

        public void Write(BinaryWriter writer)
        {
            // Write the SiteModel attributes
            writer.Write(kMajorVersion);
            writer.Write(kMinorVersionLatest);
            // writer.Write(Name);
            // writer.Write(Description);
            writer.Write(ID.ToByteArray());

            // WriteBooleanToStream(Stream, FActive);

            //WriteDoubleToStream(Stream, FMaxInterEpochDist);
            //WriteBooleanToStream(Stream, FIgnoreInvalidPositions);

            writer.Write(Grid.CellSize);

            SiteModelExtent.Write(writer);

            //FProofingRuns.WriteToStream(Stream);
            //FSiteModelDesigns.WriteToStream(Stream);

            // Write the design names list
            //FSiteModelDesignNames.SaveToStream(Stream);

            // Write the machines list
            Machines.Write(writer);

            writer.Write(LastModifiedDate.ToBinary());
        }

        public bool Read(BinaryReader reader)
        {
            // Write the SiteModel attributes
            int MajorVersion = reader.ReadInt32();
            int MinorVersion = reader.ReadInt32();

            if (!(MajorVersion == kMajorVersion && (MinorVersion == kMinorVersion)))
            {
                Log.LogError($"Unknown version number {MajorVersion}:{MinorVersion} in Read()");
                return false;
            }

            // Name = reader.ReadString();
            // Description = reader.ReadString();

            // Read the ID of the data model from the stream.
            // If the site model already has an assigned ID then
            // use this ID in favour of the ID read from the data model.
            Guid LocalID = reader.ReadGuid();

            if (ID == Guid.Empty)
            {
                ID = LocalID;
            }

            /* TODO: Is there a need for 'active' status in a sitemodel?
            Active = reader.ReadBool();
            if (!Active)
            {
                SIGLogMessage.PublishNoODS(Self, Format('Site model %d is not marked as active in the internal data model file, resetting to active', [FID]), slmcError);
                Active = true;
            }
            */

            // FMaxInterEpochDist:= ReadDoubleFromStream(Stream);
            // FIgnoreInvalidPositions:= ReadBooleanFromStream(Stream);

            double SiteModelGridCellSize = reader.ReadDouble();
            if (SiteModelGridCellSize < 0.001)
            {
                Log.LogError($"'SiteModelGridCellSize is suspicious: {SiteModelGridCellSize} for datamodel {ID}, setting to default");
                SiteModelGridCellSize = SubGridTreeConsts.DefaultCellSize; // VLPDSvcLocations.VLPD_DefaultSiteModelGridCellSize;
            }
            Grid.CellSize = SiteModelGridCellSize;

            SiteModelExtent.Read(reader);

            // FProofingRuns.ReadFromStream(Stream);
            // FSiteModelDesigns.ReadFromStream(Stream);

            // Read the design names list
            //FSiteModelDesignNames.LoadFromStream(Stream);

            // Read the machines list
            Machines.Read(reader);

            LastModifiedDate = DateTime.FromBinary(reader.ReadInt64());

            return true;
        }

        public bool SaveToPersistentStore(IStorageProxy StorageProxy)
        {
            bool Result;

            using (MemoryStream MS = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(MS))
                {
                    lock (this)
                    {
                        Write(writer);

                        Result = StorageProxy.WriteStreamToPersistentStore(ID, kSiteModelXMLFileName, FileSystemStreamType.ProductionDataXML, MS) == FileSystemErrorStatus.OK
                                 && SaveProductionDataExistanceMapToStorage(StorageProxy) == FileSystemErrorStatus.OK;
                    }
                }
            }

            if (Result)
            {
                //if (VLPDSvcLocations.VLPDTagProc_AdviseOtherServicesOfDataModelChanges)
                // {
                // Notify the SiteModel that it's attributes have changed
              //  SiteModelAttributesChangedEventSender.ModelAttributesChanged(ID);

                //        SIG_SiteModelStateEventControl.Publish(Self, sig_smscAttributesModified, FID);
                //}
            }
            else
            {
                Log.LogError($"Failed to save site model for project {ID} to persistent store");
            }

            return Result;
        }

    private bool CreateMachinesTargetValues()
    {
      // Recreate the array and alloow lazt loading of the machien event lists to occur organically
      MachinesTargetValues = new MachinesProductionEventLists(this);

      return true;

      /*
       This method is the raptor approach of recreating all the machine event lists at once. TRex will defer this
       by simply recreating the null list of machines events
      try
      {
        foreach (var machine in Machines) 
        {
          if (MachinesTargetValues[machine.InternalSiteModelMachineIndex] == null)
            MachinesTargetValues.CreateNewMachineTargetValues(machine, machine.ID);
          else
          {
            // Update the machine reference in the target values with the newly created
            // machine instance. If this is not done then code processing cell passes
            // and referencing the machine instance in the machine target values
            // (such as in ICCellPassFastEventLookUps) will reference invalid
            // machine references causing exceptions such as reported in bug 22872
            // and bug 27799
            MachinesTargetValues[machine.InternalSiteModelMachineIndex).UpdateMachineReference(machine);
          }
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError($"Exception in .CreateMachinesTargetValues: {e}");
      }

      return false;
      */
    }


        public FileSystemErrorStatus LoadFromPersistentStore(IStorageProxy StorageProxy)
        {
            Guid SavedID = ID;
            FileSystemErrorStatus Result = StorageProxy.ReadStreamFromPersistentStoreDirect(ID, kSiteModelXMLFileName, FileSystemStreamType.ProductionDataXML, out MemoryStream MS);

            if (Result == FileSystemErrorStatus.OK)
            {
                try
                {
                    if (SavedID != ID)
                    {
                        // The SiteModelID read from the FS file does not match the ID expected.

                        // RPW 31/1/11: This used to be an error with it's own error code. This is now
                        // changed to a warning, but loading of the sitemodel is allowed. This
                        // is particularly useful for testing purposes where copying around projects
                        // is much quicker than reprocessing large sets of TAG files

                        Log.LogWarning($"Site model ID read from FS file ({ID}) does not match expected ID ({SavedID}), setting to expected");
                        ID = SavedID;
                    }

                    // Prior to reading the site model from the stream, ensure that we have
                    // acquired locks to prevent access of the machine target values while the
                    // machines list is destroyed and recreated. LockMachinesTargetValues creates
                    // a list of items it obtains locks of and UnLockMachinesTargetValues releases
                    // locks against that list. This is necessary as rereading the machines may cause
                    // new machines to be created due to TAG file processing, and these new machines
                    // will not have targets values participating in the lock.

                    MS.Position = 0;
                    using (BinaryReader reader = new BinaryReader(MS, Encoding.UTF8, true))
                    {
                        lock (this)
                        {
                            Read(reader);

                            // Now read in the existance map
                            Result = LoadProductionDataExistanceMapFromStorage(StorageProxy);
                        }
                    }

                  if (!CreateMachinesTargetValues())
                    Result = FileSystemErrorStatus.UnknownErrorReadingFromFS;
                  else
                  {
                    /* TODO This type of management is not appropriate for Ignite based cache management as
                     *  list updates will cause Ignite level cache invalidation that can then cause messaging
                     *  to trigger reloading of target values/event lists

                        //Mark override lists dirty
                        for I := 0 to MachinesTargetValues.Count - 1 do
                                MachinesTargetValues.Items[I].TargetValueChanges.MarkOverrideEventListsAsOutOfDate;
                    */
                  }

                  if (Result == FileSystemErrorStatus.OK)
                    {
                        Log.LogDebug($"Site model read from FS file (ID:{ID}) succeeded");
                        Log.LogDebug($"Data model extents: {SiteModelExtent}, CellSize: {Grid.CellSize}");
                    }
                    else
                    {
                        Log.LogWarning($"Site model ID read from FS file ({ID}) failed with error {Result}");
                    }
                }
                finally
                {
                    MS.Dispose();
                }
            }

            return Result;
        }

        /// <summary>
        /// Returns a reference to the existance map for the site model. If the existance map is not yet present
        /// load it from storage/cache
        /// </summary>
        /// <returns></returns>
        public ISubGridTreeBitMask GetProductionDataExistanceMap(IStorageProxy StorageProxy)
        {
            if (existanceMap == null)
            {
                return LoadProductionDataExistanceMapFromStorage(StorageProxy) == FileSystemErrorStatus.OK ? existanceMap : null;
            }

            return existanceMap;
        }

        /// <summary>
        /// Saves the content of the existence map to storage
        /// </summary>
        /// <returns></returns>
        protected FileSystemErrorStatus SaveProductionDataExistanceMapToStorage(IStorageProxy StorageProxy)
        {
            try
            {
                // Create the new existance map instance
                ISubGridTreeBitMask localExistanceMap = existanceMap;

                // Save its content to storage
                using (MemoryStream MS = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(MS))
                    {
                        SubGridTreePersistor.Write(localExistanceMap, "ExistanceMap", 1, writer, null);
                        StorageProxy.WriteStreamToPersistentStoreDirect(ID, kSubGridExistanceMapFileName, FileSystemStreamType.SubgridExistenceMap, MS);
                    }
                }
            }
            catch // (Exception E)
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }

            return FileSystemErrorStatus.OK;
        }

        /// <summary>
        /// Retrieves the content of the existance map from storage
        /// </summary>
        /// <returns></returns>
        protected FileSystemErrorStatus LoadProductionDataExistanceMapFromStorage(IStorageProxy StorageProxy)
        {
            try
            {
        // Create the new existance map instance
                ISubGridTreeBitMask localExistanceMap = new SubGridTreeSubGridExistenceBitMask();

                // Read its content from storage 
                StorageProxy.ReadStreamFromPersistentStoreDirect(ID, kSubGridExistanceMapFileName, FileSystemStreamType.ProductionDataXML, out MemoryStream MS);
 
                try
                {
                    using (BinaryReader reader = new BinaryReader(MS))
                    {
                        SubGridTreePersistor.Read(localExistanceMap, "ExistanceMap", 1, reader, null);
                    }
                }
                finally
                {
                    MS?.Dispose();
                }

                // Replace existance map with the newly read map
                existanceMap = localExistanceMap;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorReadingFromFS;
            }

            return FileSystemErrorStatus.OK;
        }

        /// <summary>
        /// GetAdjustedDataModelSpatialExtents returns the bounding extent of the production data held in the 
        /// data model expanded to include the bounding extents of the surveyed surfaces associated with the 
        /// datamodel, excepting those identitied in the SurveyedSurfaceExclusionList
        /// </summary>
        /// <returns></returns>
        public BoundingWorldExtent3D GetAdjustedDataModelSpatialExtents(Guid[] SurveyedSurfaceExclusionList)
        {
            if (SurveyedSurfaces == null || SurveyedSurfaces.Count == 0)
                return SiteModelExtent;

            // Start with the data model extents
          BoundingWorldExtent3D SpatialExtents = new BoundingWorldExtent3D(SiteModelExtent);

          // Iterate over all non-exluded surveyed surfaces and expand the SpatialExtents as necessary
            if (SurveyedSurfaceExclusionList == null || SurveyedSurfaceExclusionList.Length == 0)
            {
                foreach (ISurveyedSurface surveyedSurface in SurveyedSurfaces)
                    SpatialExtents.Include(surveyedSurface.Extents);
            }
            else
            {
                foreach (ISurveyedSurface surveyedSurface in SurveyedSurfaces)
                {
                    if (SurveyedSurfaceExclusionList.All(x => x != surveyedSurface.ID))
                        SpatialExtents.Include(surveyedSurface.Extents);
                }
            }

            return SpatialExtents;
        }
    }
}
