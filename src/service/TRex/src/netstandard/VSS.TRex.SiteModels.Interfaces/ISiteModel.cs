using System;
using System.Collections.Generic;
using System.IO;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModel
  {
    /// <summary>
    /// Governs which TRex storage representation (mutable or immutable) the Grid member within the site model instance will supply
    /// </summary>
    StorageMutability StorageRepresentationToSupply { get; }

    void SetStorageRepresentationToSupply(StorageMutability mutability);

    IStorageProxy PrimaryStorageProxy { get; }

    Guid ID { get; set; }

    DateTime CreationDate { get; }
    DateTime LastModifiedDate { get; set; }

    double CellSize { get; }

    /// <summary>
    /// Gets/sets transient state for this site model. Transient site models are not persisted.
    /// </summary>
    bool IsTransient { get; }

    /// <summary>
    /// The grid data for this site model
    /// </summary>
    IServerSubGridTree Grid { get; }

    bool GridLoaded { get; }

    BoundingWorldExtent3D SiteModelExtent { get; }

    /// <summary>
    /// Returns a reference to the existence map for the site model. If the existence map is not yet present
    /// load it from storage/cache
    /// </summary>
    ISubGridTreeBitMask ExistenceMap { get; }

    /// <summary>
    /// Gets the loaded state of the existence map. This permits testing if an existence map is loaded without forcing
    /// the existence map to be loaded via the ExistenceMap property
    /// </summary>
    bool ExistenceMapLoaded { get; }

    /// <summary>
    /// SiteModelDesigns records all the designs that have been seen in this site model.
    /// Each site model designs records the name of the site model and the extents
    /// of the cell information that have been record for it.
    /// </summary>
    ISiteModelDesignList SiteModelDesigns { get; }

    bool SiteModelDesignsLoaded { get; }

    /// <summary>
    /// Designs records all the design surfaces that have been imported into the site model
    /// </summary>
    IDesigns Designs { get; }
    bool DesignsLoaded { get; }

    ISurveyedSurfaces SurveyedSurfaces { get; }
    bool SurveyedSurfacesLoaded { get; }

    IAlignments Alignments { get; }
    bool AlignmentsLoaded { get; }

    /// <summary>
    /// The SiteProofingRuns records all the proofing runs that have been seen in tag files for this site model.
    /// Each site model proofing run records the name of the site model, machine ID, start/end times and the extents
    /// of the cell information that have been record for it.
    /// </summary>
    ISiteProofingRunList SiteProofingRuns { get; }

    bool SiteProofingRunsLoaded { get; }

    /// <summary>
    /// SiteModelMachineDesigns records all the machineDesignNames retrieved from tag file Change events.
    /// An indexed list is maintained in the site model reported into.
    /// The event is stored with the index into the site model list.
    /// </summary>
    ISiteModelMachineDesignList SiteModelMachineDesigns { get; }
    
    bool SiteModelMachineDesignsLoaded { get; }

    IMachinesList Machines { get; }

    bool MachinesLoaded { get; }

    bool IgnoreInvalidPositions { get; set; }

    string CSIB();
    bool CSIBLoaded { get; }

    void Include(ISiteModel Source);
    void Write(BinaryWriter writer);
    void Read(BinaryReader reader);
    bool SaveMetadataToPersistentStore(IStorageProxy storageProxy);
    bool SaveToPersistentStoreForTAGFileIngest(IStorageProxy storageProxy);
    FileSystemErrorStatus LoadFromPersistentStore();

    /// <summary>
    /// GetAdjustedDataModelSpatialExtents returns the bounding extent of the production data held in the 
    /// data model expanded to include the bounding extents of the surveyed surfaces associated with the 
    /// datamodel, excepting those identified in the SurveyedSurfaceExclusionList
    /// </summary>
    /// <returns></returns>
    BoundingWorldExtent3D GetAdjustedDataModelSpatialExtents(Guid[] SurveyedSurfaceExclusionList);

    /// <summary>
    /// GetDateRange returns the chronological extents of production data in the site model.
    /// if no production data exists, then min = MaxValue and max and MinValue
    /// </summary>
    /// <returns></returns>
    (DateTime startUtc, DateTime endUtc) GetDateRange();

    /// <summary>
    /// GetAssetOnDesignPeriods returns the chronological slices where each machine was on a design.    /// </summary>
    /// <returns></returns>
    List<AssetOnDesignPeriod> GetAssetOnDesignPeriods();


    /// <summary>
    /// GetAssetOnDesignLayerPeriods returns the designs and layers used by specific machines.
    /// </summary>
    /// <returns></returns>
    List<AssetOnDesignLayerPeriod> GetAssetOnDesignLayerPeriods();

    IMachinesProductionEventLists MachinesTargetValues { get; }
    bool MachineTargetValuesLoaded { get; }

    ISiteModelMetadata MetaData { get; }

    byte GetCCAMinimumPassesValue(Guid machineUID, DateTime startDate, DateTime endDate, int layerID);
  }
}
