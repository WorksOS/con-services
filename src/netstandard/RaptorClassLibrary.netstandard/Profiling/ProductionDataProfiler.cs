using System.Collections.Generic;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
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

    private List<ProfileCell> ProfileCells = new List<ProfileCell>();
    private ISiteModel SiteModel;

    private bool PopulationControl_AnySet;
    public bool Aborted;
    public double GridDistanceBetweenProfilePoints;
    public FilteredValuePopulationControl PopulationControl;

    public CellPassFastEventLookerUpper CellPassFastEventLookerUpper { get; set; }
    private SubGridTreeBitMask PDExistenceMap;

    public ProductionDataProfiler(ISiteModel siteModel,
      IServerSubGridTree subGridTree,
      GridDataType profileTypeRequired,
      FilteredValuePopulationControl populationControl,
      SubGridTreeBitMask pDExistenceMap)
    {
      SiteModel = siteModel;
      SubGridTree = subGridTree;
      ProfileTypeRequired = profileTypeRequired;
      PopulationControl = populationControl;
      PDExistenceMap = pDExistenceMap;
    }

    private bool ReadCellPassIntoTempList;

    // FLastGetTargetValues_MachineID : TICMachineID; <== Should not be needed due to Ignite based lock free implementation

    private ProductionEventLists MachineTargetValues; // : TICSiteModelMachineTargetValues;

    /// <summary>
    /// Aborts the current profiling computation
    /// </summary>
    public void Abort() => Aborted = true;

    /// <summary>
    /// Obtains a references to the collection of event lists in the currnet sitemodel belonging to
    /// a designated machine within the sitemodel
    /// </summary>
    /// <param name="forMachineID"></param>
    /// <returns></returns>
    private ProductionEventLists GetTargetValues(short forMachineID) //TICSiteModelMachineTargetValues;
    {
      /*
        //if Debug_ExtremeLogSwitchH then
        //  SIGLogMessage.PublishNoODS(Nil, Format('In GetTargetValues', []), slmcDebug);

        if ForMachineID = FLastGetTargetValues_MachineID then
          begin
            Result := FMachineTargetValues;
            Exit;
          end;

        // Locate the machine target values of the machine we want to lock
        FMachineTargetValues := FSiteModel.MachinesTargetValues.LocateByMachineID(ForMachineID);
        Result := FMachineTargetValues;

        // If necessary, acquire the interlock on this set of machine target values
        if FPopulationControl_AnySet and Assigned(FMachineTargetValues) then
          FLastGetTargetValues_MachineID := ForMachineID;
        else
          FLastGetTargetValues_MachineID := -1;

        //if Debug_ExtremeLogSwitchH then
          SIGLogMessage.PublishNoODS(Nil, Format('Out GetTargetValues', []), slmcDebug);
      */

      // Note: The commment out implementation above is entirely concerned with l;ocking semantics around the 
      // machine events for the site model in question. TRex provides a no-lock metaphor that means these accesses
      // don't require this logic
      return SiteModel.Machines[forMachineID].TargetValueChanges;
    }
  }
}
