using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModels
{
  // <summary>
  // Represents the existence of and meta data for a site model/data model/project present in TRex.
  // It also holds references to numerous other aspects of project, such as designs, machines, surveyed surfaces,
  // and events among other things.
  // Access mechanisms are typically lock free with the only exceptions being those occasions when thread contention
  // to create a new or updated instance of some element needs to be managed.
  // </summary>
  // <remarks>
  // Note(1): This class should never be serialized over the wire to any context for any reason. All contexts requiring access
  // to a site model must use the local DIContext to access the SiteModels manager to obtain a reference to the desired site model.
  // 
  // Note(2): All site model references should be treated as immutable and ephemeral. The access period to such a reference
  // should be constrained to the life cycle of the request.
  // Each request should obtain a new site model reference to ensure it contains current versions of the information held by that site model.
  // 
  // Note(3): The site model reference obtained by a reference is not singular to that request. Multiple requests may share the
  // same site model request safely.
  // 
  // Note(4): TRex site model change notifications manage how a site model responds to mutating events made to the persistent state
  // of that site model. These changes may cause the creation of a new cloned site model that inherits elements not affected by
  // the mutating change, and will relinquish elements that have been to allow deferred/lazy loading on subsequent reference.
  // Requests referencing such site models will have consistent access to already referenced elements of the site model
  // for the duration of the request. However, non-referenced spatial data elements and their cached derivatives are actively
  // recycled during spatial data change notifications. Notwithstanding this, any actively referenced element such as a sub grid
  // or cache derivative is always consistently valid for the duration of that reference, within a request, regardless of spatial
  // data invalidation due to mutating changes, even of those referenced elements.
  // </remarks>
  public class SiteModel : ISiteModel, IBinaryReaderWriter
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModel>();

    private const string kSiteModelXMLFileName = "ProductionDataModel.XML";
    private const string kSubGridExistenceMapFileName = "SubGridExistenceMap";

    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Governs which TRex storage representation (mutable or immutable) the Grid member within the site model instance will supply
    /// By default this is assigned to Immutable. Actors responsible for mutating information in the site model (ie: TAG file ingest
    /// processors should ensure they obtain the mutable representation).
    /// </summary>
    public StorageMutability StorageRepresentationToSupply { get; private set; } = StorageMutability.Immutable;

    public void SetStorageRepresentationToSupply(StorageMutability mutability)
    {
      if (mutability != StorageRepresentationToSupply)
      {
        // Dump the Grid reference as this will need to be re-created
        grid = null;
        StorageRepresentationToSupply = mutability;
        PrimaryStorageProxy = DIContext.Obtain<ISiteModels>().PrimaryStorageProxy(mutability);
      }
    }

    public IStorageProxy PrimaryStorageProxy { get; private set; } 

    public Guid ID { get; set; } = Guid.Empty;

    public DateTime CreationDate { get; private set; }

    public DateTime LastModifiedDate { get; set; }

    public double CellSize { get; private set; } = SubGridTreeConsts.DefaultCellSize;

    /// <summary>
    /// Gets/sets transient state for this site model. Transient site models are not persisted.
    /// </summary>
    public bool IsTransient { get; private set; } = true;

    private readonly object machineLoadLockObject = new object();
    private readonly object siteProofingRunLockObject = new object();
    private readonly object siteModelMachineDesignsLockObject = new object();
    private readonly object siteModelDesignsLockObject = new object();

    private IServerSubGridTree grid;

    /// <summary>
    /// The grid data for this site model
    /// </summary>
    public IServerSubGridTree Grid => grid ?? (grid = new ServerSubGridTree(ID, StorageRepresentationToSupply) {CellSize = this.CellSize});

    public bool GridLoaded => grid != null;

    private ISubGridTreeBitMask existenceMap;

    /// <summary>
    /// Returns a reference to the existence map for the site model. If the existence map is not yet present
    /// load it from storage/cache.
    /// This will never return a null reference. In the case of a site model that does not have any spatial data within it
    /// this will return an empty existence map rather than null.
    /// </summary>
    public ISubGridTreeBitMask ExistenceMap => existenceMap ?? (existenceMap = LoadProductionDataExistenceMapFromStorage());

    /// <summary>
    /// Gets the loaded state of the existence map. This permits testing if an existence map is loaded without forcing
    /// the existence map to be loaded via the ExistenceMap property
    /// </summary>
    public bool ExistenceMapLoaded => existenceMap != null;

    /// <summary>
    /// SiteModelExtent records the 3D extents of the data stored in the site model
    /// </summary>
    public BoundingWorldExtent3D SiteModelExtent { get; } = BoundingWorldExtent3D.Inverted();

    /// <summary>
    /// Local cached copy of the coordinate system CSIB
    /// </summary>
    private string csib;

    /// <summary>
    /// The string serialized CSIB gained from adding a coordinate system from a DC or similar file
    /// to the project. This getter is responsible for accessing the information from the persistent
    /// store and caching it in the site model
    /// </summary>
    public string CSIB()
    {
      if (csib != null)
        return csib;

      if (IsTransient)
        return csib = string.Empty;

      FileSystemErrorStatus readResult = PrimaryStorageProxy.ReadStreamFromPersistentStore(ID,
          CoordinateSystemConsts.kCoordinateSystemCSIBStorageKeyName,
          FileSystemStreamType.CoordinateSystemCSIB,
          out MemoryStream csibStream);

      if (readResult != FileSystemErrorStatus.OK || csibStream == null || csibStream.Length == 0)
        return csib = string.Empty;

      using (csibStream)
      {
        csib = Encoding.ASCII.GetString(csibStream.ToArray());
        return csib;
      }
    }

    /// <summary>
    /// Gets the loaded state of the CSIB. This permits testing if a CSIB is loaded without forcing
    /// the CSIB to be loaded via the CSIB property
    /// </summary>
    public bool CSIBLoaded => csib != null;

    // MachinesTargetValues stores a list of target values, one list per machine,
    // that record how the configured target CCV and pass count settings on each
    // machine has changed over time.
    private IMachinesProductionEventLists machinesTargetValues;

    public IMachinesProductionEventLists MachinesTargetValues
    {
      // Allow lazy loading of the machine event lists to occur organically.
      // Any requests holding references to events lists will continue to do so as the lists themselves
      // wont be garbage collected until all request references to them are relinquished
      get => machinesTargetValues ?? (machinesTargetValues = new MachinesProductionEventLists(this, Machines.Count));
    }

    public bool MachineTargetValuesLoaded => machinesTargetValues != null;

    /// <summary>
    /// Provides a set of metadata attributes about this site model
    /// </summary>
    public ISiteModelMetadata MetaData => GetMetaData();

    private ISiteModelDesignList _siteModelDesigns;

    /// <summary>
    /// SiteModelDesigns records all the designs that have been seen in this site model.
    /// Each site model designs records the name of the site model and the extents
    /// of the cell information that have been record for it.
    /// </summary>
    public ISiteModelDesignList SiteModelDesigns
    {
      get
      {
        if (_siteModelDesigns == null)
        {
          lock (siteModelDesignsLockObject)
          {
            if (_siteModelDesigns == null)
            {
              var newSiteModelDesigns = new SiteModelDesignList();

              if (!IsTransient)
                newSiteModelDesigns.LoadFromPersistentStore(ID, PrimaryStorageProxy);

              _siteModelDesigns = newSiteModelDesigns;
            }
          }
        }

        return _siteModelDesigns;
      }
    }

    public bool SiteModelDesignsLoaded => _siteModelDesigns != null;

    private IDesigns _designs;

    /// <summary>
    /// Designs records all the design surfaces that have been imported into the site model
    /// </summary>
    public IDesigns Designs => _designs ?? (_designs = DIContext.Obtain<IDesignManager>().List(ID));

    public bool DesignsLoaded => _designs != null;

    private ISurveyedSurfaces _surveyedSurfaces;

    /// <summary>
    /// This is a list of TTM descriptors which indicate designs
    /// that can be used as a snapshot of an actual ground surface at a specific point in time
    /// </summary>
    public ISurveyedSurfaces SurveyedSurfaces => _surveyedSurfaces ?? (_surveyedSurfaces = DIContext.Obtain<ISurveyedSurfaceManager>().List(ID));

    public bool SurveyedSurfacesLoaded => _surveyedSurfaces != null;

    private IAlignments _alignments;

    /// <summary>
    /// alignments records all the alignment files that have been imported into the site model
    /// </summary>
    public IAlignments Alignments => _alignments ?? (_alignments = DIContext.Obtain<IAlignmentManager>().List(ID));

    public bool AlignmentsLoaded => _alignments != null;


    // The siteProofingRuns is the set of proofing runs that have been collected in this site model
    private ISiteProofingRunList siteProofingRuns;

    /// <summary>
    /// The SiteProofingRuns records all the proofing runs that have been seen in tag files for this site model.
    /// Each site model proofing run records the name of the site model, machine ID, start/end times and the extents
    /// of the cell information that have been record for it.
    /// </summary>
    public ISiteProofingRunList SiteProofingRuns
    {
      get
      {
        if (siteProofingRuns == null)
        {
          lock (siteProofingRunLockObject)
          {
            if (siteProofingRuns == null)
            {
              var newSiteProofingRuns = new SiteProofingRunList {DataModelID = ID};

              if (!IsTransient)
                newSiteProofingRuns.LoadFromPersistentStore(PrimaryStorageProxy);

              siteProofingRuns = newSiteProofingRuns;
            }
          }
        }

        return siteProofingRuns;
      }
    }

    public bool SiteProofingRunsLoaded => siteProofingRuns != null;

    /// <summary>
    /// SiteModelMachineDesigns records all the designs that have been seen in tag files for this site model.
    /// </summary>
    private ISiteModelMachineDesignList siteModelMachineDesigns;

    public ISiteModelMachineDesignList SiteModelMachineDesigns
    {
      get
      {
        if (siteModelMachineDesigns == null)
        {
          lock (siteModelMachineDesignsLockObject)
          {
            if (siteModelMachineDesigns == null)
            {
              var newSiteModelMachineDesigns = new SiteModelMachineDesignList
              {
                DataModelID = ID
              };

              if (!IsTransient)
                newSiteModelMachineDesigns.LoadFromPersistentStore(PrimaryStorageProxy);

              siteModelMachineDesigns = newSiteModelMachineDesigns;
            }
          }
        }

        return siteModelMachineDesigns;
      }
    }

    public bool SiteModelMachineDesignsLoaded => siteModelMachineDesigns != null;

    // Machines contains a list of compactor machines that this site model knows
    // about. Each machine contains a link to the machine hardware ID for the
    // appropriate machine

    private IMachinesList machines;

    public IMachinesList Machines
    {
      get
      {
        if (machines == null)
        {
          lock (machineLoadLockObject)
          {
            if (machines == null)
            {
              var newMachines = new MachinesList
              {
                DataModelID = ID
              };

              if (!IsTransient)
              {
                newMachines.LoadFromPersistentStore(PrimaryStorageProxy);
              }

              machines = newMachines;
            }
          }
        }

        return machines;
      }
    }

    public bool MachinesLoaded => machines != null;

    /// <summary>
    /// Default ignoring invalid positions to true for TAG files processing into this site model.
    /// </summary>
    public bool IgnoreInvalidPositions { get; set; } = true;


    public SiteModel()
    {
      CreationDate = DateTime.UtcNow;
      LastModifiedDate = CreationDate;

      PrimaryStorageProxy = DIContext.Obtain<ISiteModels>().PrimaryStorageProxy(StorageRepresentationToSupply);
    }

    /// <summary>
    /// Constructs a site model from an 'origin' site model that provides select information to seed the new site model
    /// </summary>
    /// <param name="originModel"></param>
    /// <param name="originFlags"></param>
    public SiteModel(ISiteModel originModel, SiteModelOriginConstructionFlags originFlags) : this()
    {
      if (originModel.IsTransient)
        throw new TRexSiteModelException("Cannot use a transient site model as an origin for constructing a new site model");

      ID = originModel.ID;
      CellSize = originModel.CellSize;
      IsTransient = false;

      CreationDate = originModel.CreationDate;
      LastModifiedDate = originModel.LastModifiedDate;

      SetStorageRepresentationToSupply(originModel.StorageRepresentationToSupply);

      grid = (originFlags & SiteModelOriginConstructionFlags.PreserveGrid) != 0
        ? originModel.Grid
        : null;

      csib = (originFlags & SiteModelOriginConstructionFlags.PreserveCsib) != 0
        ? originModel.CSIB()
        : null;

      existenceMap = originModel.ExistenceMapLoaded && (originFlags & SiteModelOriginConstructionFlags.PreserveExistenceMap) != 0
        ? originModel.ExistenceMap
        : null;

      _designs = originModel.DesignsLoaded && (originFlags & SiteModelOriginConstructionFlags.PreserveDesigns) != 0
        ? originModel.Designs
        : null;

      _surveyedSurfaces = originModel.SurveyedSurfacesLoaded && (originFlags & SiteModelOriginConstructionFlags.PreserveSurveyedSurfaces) != 0
        ? originModel.SurveyedSurfaces
        : null;

      _alignments = originModel.AlignmentsLoaded && (originFlags & SiteModelOriginConstructionFlags.PreserveAlignments) != 0
        ? originModel.Alignments
        : null;

      machines = originModel.MachinesLoaded && (originFlags & SiteModelOriginConstructionFlags.PreserveMachines) != 0
        ? originModel.Machines
        : null;

      siteProofingRuns = originModel.SiteProofingRunsLoaded && (originFlags & SiteModelOriginConstructionFlags.PreserveProofingRuns) != 0
        ? originModel.SiteProofingRuns
        : null;

      siteModelMachineDesigns = originModel.SiteModelMachineDesignsLoaded && (originFlags & SiteModelOriginConstructionFlags.PreserveMachineDesigns) != 0
        ? originModel.SiteModelMachineDesigns
        : null;

      // Machine target values are an extension vector from machines. If the machine have not changed
      machinesTargetValues = originModel.MachineTargetValuesLoaded && (originFlags & SiteModelOriginConstructionFlags.PreserveMachineTargetValues) != 0
        ? originModel.MachinesTargetValues
        : null;

      _siteModelDesigns = originModel.SiteModelDesignsLoaded && (originFlags & SiteModelOriginConstructionFlags.PreserveSiteModelDesigns) != 0
        ? originModel.SiteModelDesigns
        : null;

      // Reload the bits that need to be reloaded
      LoadFromPersistentStore();
    }

    public SiteModel(Guid id, bool isTransient = true) : this()
    {
      ID = id;
      IsTransient = isTransient;

      // Allow existence map loading to be deferred/lazy on reference
      existenceMap = null;
    }

    public SiteModel(Guid id, double cellSize) : this(id)
    {
      CellSize = cellSize;
      Grid.CellSize = cellSize;
    }

    public void Include(ISiteModel Source)
    {
      // SiteModel extents
      SiteModelExtent.Include(Source.SiteModelExtent);

      // Proofing runs
      if (Source.SiteProofingRunsLoaded)
        for (var i = 0; i < Source.SiteProofingRuns.Count; i++)
        {
          var proofingRun = Source.SiteProofingRuns[i];

          if (!SiteProofingRuns.CreateAndAddProofingRun(proofingRun.Name, proofingRun.MachineID, proofingRun.StartTime, proofingRun.EndTime, proofingRun.Extents))
          {
            SiteProofingRuns[i].Extents.Include(proofingRun.Extents);

            if (SiteProofingRuns[i].StartTime > proofingRun.StartTime)
              SiteProofingRuns[i].StartTime = proofingRun.StartTime;

            if (SiteProofingRuns[i].EndTime < proofingRun.EndTime)
              SiteProofingRuns[i].EndTime = proofingRun.EndTime;
          }
        }

        LastModifiedDate = Source.LastModifiedDate;
    }

    public void Write(BinaryWriter writer)
    {
      // Write the SiteModel attributes
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.Write(ID.ToByteArray());
      writer.Write(CreationDate.ToBinary());

      //WriteBooleanToStream(Stream, FIgnoreInvalidPositions);

      writer.Write(CellSize);

      SiteModelExtent.Write(writer);

      writer.Write(LastModifiedDate.ToBinary());
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    public void Read(BinaryReader reader)
    {
      // Read the SiteModel attributes

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      // Read the ID of the data model from the stream.
      // If the site model already has an assigned ID then
      // use this ID in favor of the ID read from the data model.
      Guid LocalID = reader.ReadGuid();

      if (ID == Guid.Empty)
      {
        ID = LocalID;
      }

      CreationDate = DateTime.FromBinary(reader.ReadInt64());

      // FIgnoreInvalidPositions:= ReadBooleanFromStream(Stream);

      CellSize = reader.ReadDouble();
      if (CellSize < 0.001)
      {
        Log.LogError($"SiteModelGridCellSize is suspicious: {CellSize} for datamodel {ID}, setting to default: {SubGridTreeConsts.DefaultCellSize}");
        CellSize = SubGridTreeConsts.DefaultCellSize;
      }

      SiteModelExtent.Read(reader);

      LastModifiedDate = DateTime.FromBinary(reader.ReadInt64());
    }

    /// <summary>
    /// Saves only the core metadata about the site model to the persistent store
    /// </summary>
    /// <param name="storageProxy"></param>
    /// <returns></returns>
    public bool SaveMetadataToPersistentStore(IStorageProxy storageProxy)
    {
      if (storageProxy.WriteStreamToPersistentStore(ID, kSiteModelXMLFileName, FileSystemStreamType.ProductionDataXML, this.ToStream(), this) == FileSystemErrorStatus.OK)
      {
        storageProxy.Commit();
        return true;
      }

      Log.LogError($"Failed to save site model metadata for site model {ID} to persistent store");
      return false;
    }

    /// <summary>
    /// Save the site model metadata and core mutated state driven by TAG file ingest
    /// </summary>
    /// <param name="storageProxy"></param>
    /// <returns></returns>
    public bool SaveToPersistentStoreForTAGFileIngest(IStorageProxy storageProxy)
    {
      bool Result = true;

      lock (this)
      {
        if (storageProxy.WriteStreamToPersistentStore(ID, kSiteModelXMLFileName, FileSystemStreamType.ProductionDataXML, this.ToStream(), this) != FileSystemErrorStatus.OK)
        {
          Log.LogError($"Failed to save site model metadata for site model {ID} to persistent store");
          Result = false;
        }

        if (ExistenceMapLoaded && SaveProductionDataExistenceMapToStorage(storageProxy) != FileSystemErrorStatus.OK)
        {
          Log.LogError($"Failed to save existence map for site model {ID} to persistent store");
          Result = false;
        }

        machines?.SaveToPersistentStore(storageProxy);
        siteProofingRuns?.SaveToPersistentStore(storageProxy);
        siteModelMachineDesigns?.SaveToPersistentStore(storageProxy);
        _siteModelDesigns?.SaveToPersistentStore(ID, storageProxy);
      }

      if (!Result)
        Log.LogError($"Failed to save site model for project {ID} to persistent store");

      return Result;
    }

    public FileSystemErrorStatus LoadFromPersistentStore()
    {
      var Result = PrimaryStorageProxy.ReadStreamFromPersistentStore(ID, kSiteModelXMLFileName, FileSystemStreamType.ProductionDataXML, out MemoryStream MS);

      if (Result == FileSystemErrorStatus.OK && MS != null)
      {
        using (MS)
        {
          MS.Position = 0;
          using (var reader = new BinaryReader(MS, Encoding.UTF8, true))
          {
            lock (this)
            {
              Read(reader);
            }
          }

          if (Result == FileSystemErrorStatus.OK)
            Log.LogInformation($"Site model read (ID:{ID}) succeeded. Extents: {SiteModelExtent}, CellSize: {CellSize}");
          else
            Log.LogWarning($"Site model ID read ({ID}) failed with error {Result}");
        }
      }

      return Result;
    }

    /// <summary>
    /// Saves the content of the existence map to storage
    /// </summary>
    /// <returns></returns>
    private FileSystemErrorStatus SaveProductionDataExistenceMapToStorage(IStorageProxy storageProxy)
    {
      var result = FileSystemErrorStatus.OK;

      if (existenceMap != null)
        result = storageProxy.WriteStreamToPersistentStore(ID, kSubGridExistenceMapFileName, FileSystemStreamType.SubgridExistenceMap, existenceMap.ToStream(), existenceMap);

      return result;
    }

    /// <summary>
    /// Retrieves the content of the existence map from storage
    /// </summary>
    /// <returns></returns>
    private SubGridTreeSubGridExistenceBitMask LoadProductionDataExistenceMapFromStorage()
    {
      var localExistenceMap = new SubGridTreeSubGridExistenceBitMask();

      // Read its content from storage 
      var readResult = PrimaryStorageProxy.ReadStreamFromPersistentStore(ID, kSubGridExistenceMapFileName, FileSystemStreamType.ProductionDataXML, out MemoryStream MS);

      if (MS == null)
        Log.LogInformation($"Attempt to read existence map for site model {ID} failed [with result {readResult}] as the map does not exist, creating new existence map");
      else
        localExistenceMap.FromStream(MS);

      // Replace existence map with the newly read map
      return localExistenceMap;
    }

    /// <summary>
    /// GetAdjustedDataModelSpatialExtents returns the bounding extent of the production data held in the 
    /// data model expanded to include the bounding extents of the surveyed surfaces associated with the 
    /// datamodel, excepting those identified in the SurveyedSurfaceExclusionList
    /// </summary>
    /// <returns></returns>
    public BoundingWorldExtent3D GetAdjustedDataModelSpatialExtents(Guid[] SurveyedSurfaceExclusionList)
    {
      // Start with the data model extents
      BoundingWorldExtent3D SpatialExtents = new BoundingWorldExtent3D(SiteModelExtent);

      if ((SurveyedSurfaces?.Count ?? 0) > 0)
      {
        // Iterate over all non-excluded surveyed surfaces and expand the SpatialExtents as necessary
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
      }

      return SpatialExtents;
    }

    /// <summary>
    /// GetDateRange returns the chronological extents of production data in the site model.
    /// if no production data exists, then min = MaxValue and max and MinValue
    /// </summary>
    /// <returns></returns>
    public (DateTime startUtc, DateTime endUtc) GetDateRange()
    {
      DateTime minDate = Consts.MAX_DATETIME_AS_UTC;
      DateTime maxDate = Consts.MIN_DATETIME_AS_UTC;

      foreach (var machine in Machines)
      {
        var events = MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents;
        if (events.Count() > 0)
        {
          events.GetStateAtIndex(0, out DateTime eventDateFirst, out _);
          if (minDate > eventDateFirst)
            minDate = eventDateFirst;
          if (maxDate < eventDateFirst)
            maxDate = eventDateFirst;

          if (events.Count() > 1)
          {
            var eventDateLast = events.LastStateDate();
            if (maxDate < eventDateLast)
              maxDate = eventDateLast;
          }
        }
      }

      return (minDate, maxDate);
    }

    /// <summary>
    /// GetAssetOnDesignPeriods returns design changes for each machine.
    ///    We remove any duplicates (occurs at start, where a period of time is missing between tag files)
    /// C:\VSS\Gen3\NonMerinoApps\VSS.Velociraptor\Velociraptor\VLPD\PS\PSNode.MachineDesigns.RPC.Execute.pas
    /// </summary>
    /// <returns></returns>
    public List<AssetOnDesignPeriod> GetAssetOnDesignPeriods()
    {
      var assetOnDesignPeriods = new List<AssetOnDesignPeriod>();

      foreach (var machine in Machines)
      {
        var events = MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents;

        var priorMachineDesignId = int.MinValue;
        var priorDateTime = Consts.MIN_DATETIME_AS_UTC;
        for (var i = 0; i < events.Count(); i++)
        {
          events.GetStateAtIndex(i, out var dateTime, out var machineDesignId);
          if (machineDesignId < 0)
          {
            Log.LogError($"{nameof(GetAssetOnDesignPeriods)}: Invalid machineDesignId in DesignNameChange event. machineID: {machine.ID} eventDate: {dateTime} ");
            continue;
          }

          if (priorMachineDesignId != int.MinValue && machineDesignId != priorMachineDesignId)
          {
            var machineDesign = SiteModelMachineDesigns.Locate(priorMachineDesignId);
            assetOnDesignPeriods.Add(new AssetOnDesignPeriod(machineDesign?.Name ?? "unknown",
              Consts.kNoDesignNameID, Consts.NULL_LEGACY_ASSETID, priorDateTime, Consts.MAX_DATETIME_AS_UTC, machine.ID));
          }

          // where multi events for same design -  want to retain startDate of first
          if (priorMachineDesignId != machineDesignId)
          {
            priorMachineDesignId = machineDesignId;
            priorDateTime = dateTime;
          }
        }

        if (priorMachineDesignId != int.MinValue)
        {
          var machineDesign = SiteModelMachineDesigns.Locate(priorMachineDesignId);
          assetOnDesignPeriods.Add(new AssetOnDesignPeriod(machineDesign?.Name ?? "unknown",
            Consts.kNoDesignNameID, Consts.NULL_LEGACY_ASSETID, priorDateTime, Consts.MAX_DATETIME_AS_UTC, machine.ID));
        }
      }

      return assetOnDesignPeriods;
    }

    /// <summary>
    /// GetAssetOnDesignLayerPeriods returns the designs and layers used by specific machines.
    /// C:\VSS\Gen3\NonMerinoApps\VSS.Velociraptor\Velociraptor\SVO\ProductionServer\SVOICSiteModels.pas
    /// As per Raymond: it is guaranteed that start/stop will occur as pairs to form a reportingPeriod,
    ///                 AND there will be NO events outside of these pairs
    ///                 AND at startEnd, status of all event types will be recited e.g. last on layer1 and designA
    /// </summary>
    /// <returns></returns>
    public List<AssetOnDesignLayerPeriod> GetAssetOnDesignLayerPeriods()
    {
      var assetOnDesignLayerPeriods = new List<AssetOnDesignLayerPeriod>();
      foreach (var machine in Machines)
      {
        var startStopEvents = MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents;
        var layerEventCount = MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.Count();
        if (layerEventCount == 0 || MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.Count() == 0)
          continue;

        for (int startStopEventIndex = 1; startStopEventIndex < startStopEvents.Count(); startStopEventIndex += 2)
        {
          startStopEvents.GetStateAtIndex(startStopEventIndex - 1, out var startReportingPeriod, out _);
          startStopEvents.GetStateAtIndex(startStopEventIndex, out var endReportingPeriod, out _);

          // identify layer changes within a report period which will likely overlap reporting periods.
          var priorLayerId = MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents
            .GetValueAtDate(startReportingPeriod, out var layerStateChangeIndex, ushort.MaxValue);
          var priorMachineDesignId = MachinesTargetValues[machine.InternalSiteModelMachineIndex]
            .MachineDesignNameIDStateEvents.GetValueAtDate(startReportingPeriod, out int _, Consts.kNoDesignNameID);
          if (priorLayerId == ushort.MaxValue || layerStateChangeIndex < 0)
            layerStateChangeIndex = 0; // no layer events found at or before startReportingPeriod
          else
            layerStateChangeIndex += 1;

          var priorLayerChangeTime = startReportingPeriod;
          var thisLayerChangeTime = startReportingPeriod;
          for (; thisLayerChangeTime < endReportingPeriod && layerStateChangeIndex < layerEventCount; layerStateChangeIndex++)
          {
            MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents
              .GetStateAtIndex(layerStateChangeIndex, out thisLayerChangeTime, out ushort nextLayerId);

            if (priorLayerId != ushort.MaxValue)
            {
              var machineDesign = SiteModelMachineDesigns.Locate(priorMachineDesignId);
              assetOnDesignLayerPeriods.Add(new AssetOnDesignLayerPeriod(Consts.NULL_LEGACY_ASSETID,
                Consts.kNoDesignNameID,
                priorLayerId, priorLayerChangeTime,
                thisLayerChangeTime <= endReportingPeriod ? thisLayerChangeTime : endReportingPeriod, machine.ID,
                machineDesign?.Name ?? "unknown"));
            }

            priorMachineDesignId = MachinesTargetValues[machine.InternalSiteModelMachineIndex]
              .MachineDesignNameIDStateEvents.GetValueAtDate(thisLayerChangeTime, out int _, Consts.kNoDesignNameID);
            priorLayerChangeTime = thisLayerChangeTime;
            priorLayerId = nextLayerId;
          }

          // event earlier in report period, this covers to end of period
          if (layerStateChangeIndex == layerEventCount && thisLayerChangeTime < endReportingPeriod)
          {
            var machineDesign = SiteModelMachineDesigns.Locate(priorMachineDesignId);
            assetOnDesignLayerPeriods.Add(new AssetOnDesignLayerPeriod(Consts.NULL_LEGACY_ASSETID,
              Consts.kNoDesignNameID, priorLayerId, priorLayerChangeTime,
              endReportingPeriod, machine.ID, machineDesign?.Name ?? "unknown"));
          }
        }
      }

      return new List<AssetOnDesignLayerPeriod>(assetOnDesignLayerPeriods);
    }

    /// <summary>
    /// Returns simple metadata about the site model
    /// </summary>
    /// <returns></returns>
    private SiteModelMetadata GetMetaData()
    {
      return new SiteModelMetadata
      {
        ID = ID,
        CreationDate = CreationDate,
        LastModifiedDate = LastModifiedDate,
        SiteModelExtent = new BoundingWorldExtent3D(SiteModelExtent),
        MachineCount = Machines?.Count ?? 0,
        DesignCount = Designs?.Count ?? 0,
        SurveyedSurfaceCount = SurveyedSurfaces?.Count ?? 0,
        AlignmentCount = Alignments?.Count ?? 0,
      };
    }
  }
}
