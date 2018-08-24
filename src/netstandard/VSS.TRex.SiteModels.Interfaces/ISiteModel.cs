using System;
using System.IO;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModel
  {
    Guid ID { get; set; }

    DateTime LastModifiedDate { get; set; }

    /// <summary>
    /// The grid data for this site model
    /// </summary>
    IServerSubGridTree Grid { get; }

    BoundingWorldExtent3D SiteModelExtent { get; }

    ISubGridTreeBitMask ExistanceMap { get; }

    /// <summary>
    /// SiteModelDesigns records all the designs that have been seen in this sitemodel.
    /// Each site model designs records the name of the site model and the extents
    /// of the cell information that have been record for it.
    /// </summary>
    ISiteModelDesignList SiteModelDesigns { get; }

    ISurveyedSurfaces SurveyedSurfaces { get; }
    IMachinesList Machines { get; set; }
    bool IgnoreInvalidPositions { get; set; }

    string CSIB();

    void Include(ISiteModel Source);
    void Write(BinaryWriter writer);
    bool Read(BinaryReader reader);
    bool SaveToPersistentStore(IStorageProxy StorageProxy);
    FileSystemErrorStatus LoadFromPersistentStore(IStorageProxy StorageProxy);

    /// <summary>
    /// Returns a reference to the existance map for the site model. If the existance map is not yet present
    /// load it from storage/cache
    /// </summary>
    /// <returns></returns>
    ISubGridTreeBitMask GetProductionDataExistanceMap(IStorageProxy StorageProxy);

    /// <summary>
    /// GetAdjustedDataModelSpatialExtents returns the bounding extent of the production data held in the 
    /// data model expanded to include the bounding extents of the surveyed surfaces associated with the 
    /// datamodel, excepting those identitied in the SurveyedSurfaceExclusionList
    /// </summary>
    /// <returns></returns>
    BoundingWorldExtent3D GetAdjustedDataModelSpatialExtents(Guid[] SurveyedSurfaceExclusionList);

    IMachinesProductionEventLists MachinesTargetValues { get; }
  }
}
