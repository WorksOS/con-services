using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Performs profiling operations across the grid of cells that describe th eproduction data held in datamodel
  /// </summary>
  public class ProductionDataProfiler
  {
    private IServerSubGridTree SubGridTree;

    private GridDataType ProfileTypeRequired;

    // USED ==> public TICProfileCellList ProfileCells;
    private ISiteModel SiteModel;
    private bool PopulationControl_AnySet;
    public bool Aborted;
    public double GridDistanceBetweenProfilePoints;
    public FilteredValuePopulationControl PopulationControl;

    // private SubGridTreeBitMask OverallExistenceMap; **** NOT USED ****
    // private SubGridTreeBitMask PDExistenceMap; **** NOT USED ****

    // USED ==> FCellPassFastEventLookerUpper : TICCellPassFastEventLookerUpper; 

    public ProductionDataProfiler(ISiteModel siteModel,
      IServerSubGridTree subGridTree,
    //const AOverallExistenceMap : TSubGridTreeBitMask; Not used?
    //const APDExistenceMap : TSubGridTreeBitMask; Not used?
    GridDataType profileTypeRequired,
    FilteredValuePopulationControl populationControl)
    {
      SiteModel = siteModel;
      SubGridTree = subGridTree;
      ProfileTypeRequired = profileTypeRequired;
      PopulationControl = populationControl;
    }

  /*
  // FLastGetTargetValues_MachineID : TICMachineID; <== Should not be need due to Ignite based lock free implementation
  FMachineTargetValues : TICSiteModelMachineTargetValues;

  TempPasses : TICFilteredPassDataArray;
  TempPassesSize, TempPassesSizeDiv2 : Integer;
  TempPassCount : Integer;
  TempPass : TICFilteredPassData;
  TempFilteredPassFlags : Array of Boolean;

  private bool ReadCellPassIntoTempList;

  Function GetTargetValues(const ForMachineID : TICMachineID) : TICSiteModelMachineTargetValues;

//  public
  property CellPassFastEventLookerUpper : TICCellPassFastEventLookerUpper read FCellPassFastEventLookerUpper write FCellPassFastEventLookerUpper;

  function BuildCellPassProfile(const CellFilter : TICGridDataCellSelectionFilter;
                                const GridDataCache : TICDataStoreCache;
                                const NEECoords     : TCSConversionCoordinates;
                                const Design : TVLPDDesignDescriptor) : Boolean;

  function BuildLiftProfileFromInitialLayer(const PassFilter       : TICGridDataPassFilter;
                                            const CellFilter : TICGridDataCellSelectionFilter;
                                            const LiftBuildSettings: TICLiftBuildSettings;
                                            const GridDataCache    : TICDataStoreCache;
                                            CellPassIterator       : TSubGridSegmentCellPassIterator): Boolean;

  function BuildLiftsForCell(const CallerID : TCallerIDs;
                             const Cell: TICProfileCell;
                             const ReturnPasses: Boolean;
                             const LiftBuildSettings: TICLiftBuildSettings;
                             const ClientGrid : TICSubGridTreeLeafSubGridBase;
                             const AssignmentContext : TICSubGridFilteredValueAssignmentContext;
                             CellPassIterator : TSubGridSegmentCellPassIterator;
                             const ReturnIndividualFilteredValueSelection : Boolean;
                             const PassFilter : TICGridDataPassFilter;
                             var FilteredPassCountOfTopMostLayer : Integer;
                             var FilteredHalfCellPassCountOfTopMostLayer : Integer): Boolean;
*/

    /// <summary>
    /// Aborts the current profiling computation
    /// </summary>
    public void Abort() => Aborted = true;
  }

  /*

type
  TICServerProfiler = class
  private
    FProfileCells: TICProfileCellList;

    FCellPassFastEventLookerUpper : TICCellPassFastEventLookerUpper;

    FLastGetTargetValues_MachineID : TICMachineID;
    FMachineTargetValues : TICSiteModelMachineTargetValues;

    TempPasses : TICFilteredPassDataArray;
    TempPassesSize, TempPassesSizeDiv2 : Integer;
    TempPassCount : Integer;
    TempPass : TICFilteredPassData;
    TempFilteredPassFlags : Array of Boolean;

    ReadCellPassIntoTempList : Boolean;

    Function GetTargetValues(const ForMachineID : TICMachineID) : TICSiteModelMachineTargetValues;

  public
    property Profile: TICProfileCellList read FProfileCells;
    property CellPassFastEventLookerUpper : TICCellPassFastEventLookerUpper read FCellPassFastEventLookerUpper write FCellPassFastEventLookerUpper;

    function BuildCellPassProfile(const CellFilter : TICGridDataCellSelectionFilter;
                                  const GridDataCache : TICDataStoreCache;
                                  const NEECoords     : TCSConversionCoordinates;
                                  const Design : TVLPDDesignDescriptor) : Boolean;

    function BuildLiftProfileFromInitialLayer(const PassFilter       : TICGridDataPassFilter;
                                              const CellFilter : TICGridDataCellSelectionFilter;
                                              const LiftBuildSettings: TICLiftBuildSettings;
                                              const GridDataCache    : TICDataStoreCache;
                                              CellPassIterator       : TSubGridSegmentCellPassIterator): Boolean;

    function BuildLiftsForCell(const CallerID : TCallerIDs;
                               const Cell: TICProfileCell;
                               const ReturnPasses: Boolean;
                               const LiftBuildSettings: TICLiftBuildSettings;
                               const ClientGrid : TICSubGridTreeLeafSubGridBase;
                               const AssignmentContext : TICSubGridFilteredValueAssignmentContext;
                               CellPassIterator : TSubGridSegmentCellPassIterator;
                               const ReturnIndividualFilteredValueSelection : Boolean;
                               const PassFilter : TICGridDataPassFilter;
                               var FilteredPassCountOfTopMostLayer : Integer;
                               var FilteredHalfCellPassCountOfTopMostLayer : Integer): Boolean;
  end;
   */
}
