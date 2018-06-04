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

    //private ProductionEventLists MachineTargetValues; // : TICSiteModelMachineTargetValues;

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
    private ProductionEventLists GetTargetValues(short forMachineID) => SiteModel.Machines[forMachineID].TargetValueChanges;
  }
}
