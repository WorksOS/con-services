using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.Types;

namespace RaptorClassLibrary.Tests.netcore.Profiling
{
  /// <summary>
  /// Performs profiling operations across the grid of cells that describe th eproduction data held in datamodel
  /// </summary>
    public class ProductionDataProfiler
  {
    private ServerSubGridTree SubGridTree;
    private GridDataType ProfileTypeRequired;
    // USED ==> FProfileCells: TICProfileCellList;
    private ISiteModel SiteModel;
    /*
    // USED ==> FCellPassFastEventLookerUpper : TICCellPassFastEventLookerUpper; 

    // private SubGridTreeBitMask OverallExistenceMap; **** NOT USED ****
    // private SubGridTreeBitMask PDExistenceMap; **** NOT USED ****

    // FLastGetTargetValues_MachineID : TICMachineID; <== Should not be need due to Ignite based lock free implementation
    FMachineTargetValues : TICSiteModelMachineTargetValues;

    private bool PopulationControl_AnySet;

    TempPasses : TICFilteredPassDataArray;
    TempPassesSize, TempPassesSizeDiv2 : Integer;
    TempPassCount : Integer;
    TempPass : TICFilteredPassData;
    TempFilteredPassFlags : Array of Boolean;

    private bool ReadCellPassIntoTempList;

    Function GetTargetValues(const ForMachineID : TICMachineID) : TICSiteModelMachineTargetValues;

//  public
//    property Profile: TICProfileCellList read FProfileCells;
    property CellPassFastEventLookerUpper : TICCellPassFastEventLookerUpper read FCellPassFastEventLookerUpper write FCellPassFastEventLookerUpper;
    public bool Aborted;
    public double GridDistanceBetweenProfilePoints;
    public FilteredValuePopulationControl PopulationControl;

    constructor Create(SiteModel: TICSiteModel;
                       ASubGridTree: TICServerSubGridTree;
                       const AOverallExistenceMap : TSubGridTreeBitMask;
                       const APDExistenceMap : TSubGridTreeBitMask;
                       const AProfileTypeRequired: TICGridDataType;
                       const APopulationControl : TFilteredValuePopulationControl);

    //destructor Destroy; override;

    Class Function PreparePopulationControl(const AProfileTypeRequired: TICGridDataType;
                                            const LiftBuildSettings: TICLiftBuildSettings;
                                            const PassFilter : TICGridDataPassFilter;
                                            const ClientGrid : TICSubGridTreeLeafSubGridBase) : TFilteredValuePopulationControl; Overload;

    Class Function PreparePopulationControl(const AProfileTypeRequired: TICGridDataType;
                                            const LiftBuildSettings: TICLiftBuildSettings;
                                            const PassFilter : TICGridDataPassFilter) : TFilteredValuePopulationControl; Overload;

    class procedure CalculateFlags(const AProfileTypeRequired: TICGridDataType;
                                   const LiftBuildSettings: TICLiftBuildSettings;
                                   var CompactionSummaryInLiftBuildSettings,
                                       WorkInProgressSummaryInLiftBuildSettings,
                                       ThicknessInProgressInLiftBuildSettings: Boolean);

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

    Procedure Abort;

*/

  }

  /*
uses
  Classes,
  Windows,
  SVOICProfileCell,
  Pas_Misc,
  Pas_Decl,
  SVOICDecls,
  ProductionServer_TLB,
  SVOICGridCell,
  SVOICLiftBuildSettings,
  SVOICSubGridTrees,
  SVOICFiltersDecls,
  ICServerSubGridTree,
  ICCellPassFastEventLookUps,
  SVOICFilters,
  ICServerSubGridTreeIterator,
  ICPopulationControlDecls,
  ICSiteModelMachineTargets,
  ICMachineTargetCCVs,
  SVOICCellSelectionFilters,
  SVOICDataStoreCache,
  SVOICSiteModels,
  PSNode.Coordinates.RPC,
  VLPDDecls,
  SubGridTrees;

type
  TICServerProfiler = class
  private
    FAborted : Boolean;
    FSubGridTree: TICServerSubGridTree;
    FProfileTypeRequired: TICGridDataType;
    FProfileCells: TICProfileCellList;
    FSiteModel: TICSiteModel;

    FCellPassFastEventLookerUpper : TICCellPassFastEventLookerUpper;

    FOverallExistenceMap : TSubGridTreeBitMask;
    FPDExistenceMap : TSubGridTreeBitMask;

    FGridDistanceBetweenProfilePoints : Double;

    FLastGetTargetValues_MachineID : TICMachineID;
    FMachineTargetValues : TICSiteModelMachineTargetValues;
// RCE 36155    FMachineTargetValuesEventsLocked : Boolean;

    FPopulationControl : TFilteredValuePopulationControl;
    FPopulationControl_AnySet : Boolean;

    TempPasses : TICFilteredPassDataArray;
    TempPassesSize, TempPassesSizeDiv2 : Integer;
    TempPassCount : Integer;
    TempPass : TICFilteredPassData;
    TempFilteredPassFlags : Array of Boolean;

    ReadCellPassIntoTempList : Boolean;

    Debug_ExtremeLogSwitchE : Boolean;
    Debug_ExtremeLogSwitchF : Boolean;
    Debug_ExtremeLogSwitchG : Boolean;
    Debug_ExtremeLogSwitchH : Boolean;

    Function GetTargetValues(const ForMachineID : TICMachineID) : TICSiteModelMachineTargetValues;
// RCE 36155    procedure AcquirePopulationFilterValuesInterlock; inline;
// RCE 36155    procedure ReleasePopulationFilterValuesInterlock; inline;

  public
    property Profile: TICProfileCellList read FProfileCells;
    property CellPassFastEventLookerUpper : TICCellPassFastEventLookerUpper read FCellPassFastEventLookerUpper write FCellPassFastEventLookerUpper;
    property Aborted : Boolean read FAborted;
    property GridDistanceBetweenProfilePoints : double read FGridDistanceBetweenProfilePoints;
    property PopulationControl : TFilteredValuePopulationControl read FPopulationControl write FPopulationControl;

    constructor Create(SiteModel: TICSiteModel;
                       ASubGridTree: TICServerSubGridTree;
                       const AOverallExistenceMap : TSubGridTreeBitMask;
                       const APDExistenceMap : TSubGridTreeBitMask;
                       const AProfileTypeRequired: TICGridDataType;
                       const APopulationControl : TFilteredValuePopulationControl);

    destructor Destroy; override;

    Class Function PreparePopulationControl(const AProfileTypeRequired: TICGridDataType;
                                            const LiftBuildSettings: TICLiftBuildSettings;
                                            const PassFilter : TICGridDataPassFilter;
                                            const ClientGrid : TICSubGridTreeLeafSubGridBase) : TFilteredValuePopulationControl; Overload;

    Class Function PreparePopulationControl(const AProfileTypeRequired: TICGridDataType;
                                            const LiftBuildSettings: TICLiftBuildSettings;
                                            const PassFilter : TICGridDataPassFilter) : TFilteredValuePopulationControl; Overload;

    class procedure CalculateFlags(const AProfileTypeRequired: TICGridDataType;
                                   const LiftBuildSettings: TICLiftBuildSettings;
                                   var CompactionSummaryInLiftBuildSettings,
                                       WorkInProgressSummaryInLiftBuildSettings,
                                       ThicknessInProgressInLiftBuildSettings: Boolean);

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

    Procedure Abort;
  end;
   */
}
