using System.Collections;
using VSS.TRex.Cells;
using VSS.TRex.Filters.Models;

namespace VSS.TRex.Filters.Interfaces
{
  public interface ICellPassAttributeFilter : ICellPassAttributeFilterModel
  {
    object /*ISiteModel*/ SiteModel { get; set; }

    /// <summary>
    /// The machines present in the filter represented as an array of internal machine IDs specific to the site model the filter is being applied to
    /// </summary>
    short[] MachineIDs { get; set; }

    /// <summary>
    /// The machines present in the filter represented as a bit set
    /// </summary>
    BitArray MachineIDSet { get; set; }

    bool LastRecordedCellPassSatisfiesFilter { get; }

    bool AnyFilterSelections { get; }
    bool AnyMachineEventFilterSelections { get; }
    bool AnyNonMachineEventFilterSelections { get; }

    void ClearFilter();
    void ClearVibeState();

    /// <summary>
    /// Compare one filter with another for the purpose of ordering them in caching lists
    /// </summary>
    /// <param name="AFilter"></param>
    /// <returns></returns>
    int CompareTo(ICellPassAttributeFilter AFilter);

    void ClearDesigns();
    void ClearElevationRange();
    void ClearElevationType();
    void ClearGPSAccuracy();
    void ClearTemperatureRange();
    void ClearPassCountRange();
    void ClearGPSTolerance();
    void ClearGuidanceMode();
    void ClearLayerID();
    void ClearLayerState();
    void Assign(ICellPassAttributeFilter Source);
    void ClearCompactionMachineOnlyRestriction();
    void ClearMachineDirection();
    void ClearMachines();
    void ClearMinElevationMapping();
    void ClearPassType();
    void ClearPositioningTech();
    void ClearSurveyedSurfaceExclusionList();
    void ClearTime();
    bool FilterPass(ref CellPass PassValue, ICellPassAttributeFilterProcessingAnnex filterAnnex);
    bool FilterPass(ref FilteredPassData PassValue, ICellPassAttributeFilterProcessingAnnex filterAnnex);
    bool FilterPassUsingTemperatureRange(ref CellPass PassValue);
    bool FilterPassUsingTimeOnly(ref CellPass PassValue);
    bool FilterPass_MachineEvents(ref FilteredPassData PassValue);
    bool FilterPass_NoMachineEvents(ref CellPass PassValue, ICellPassAttributeFilterProcessingAnnex filterAnnex);

    /// <summary>
    /// Converts an array of GUIDs representing machine identifiers into a BitArray encoding a bit set of
    /// internal machine IDs relative to this site model
    /// </summary>
    void InitialiseMachineIDsSet();

    bool FilterMultiplePasses(CellPass[] passValues,
      int PassValueCount,
      FilteredMultiplePassInfo filteredPassInfo,
      ICellPassAttributeFilterProcessingAnnex filterAnnex);

    bool ExcludeSurveyedSurfaces();
    string ActiveFiltersText();

    bool FilterSinglePass(CellPass[] passValues,
      int passValueCount,
      bool wantEarliestPass,
      ref FilteredSinglePassInfo filteredPassInfo,
      object profileCell,
      bool performAttributeSubFilter,
      ICellPassAttributeFilterProcessingAnnex filterAnnex);

    /// <summary>
    /// FilterSinglePass selects a single passes from the list of passes in
    /// PassValues where PassValues contains the entire list of passes for
    /// a cell in the database.
    /// </summary>
    /// <param name="filteredPassValues"></param>
    /// <param name="passValueCount"></param>
    /// <param name="wantEarliestPass"></param>
    /// <param name="filteredPassInfo"></param>
    /// <param name="profileCell"></param>
    /// <param name="performAttributeSubFilter"></param>
    /// <param name="filterAnnex"></param>
    /// <returns></returns>
    bool FilterSinglePass(FilteredPassData[] filteredPassValues,
      int passValueCount,
      bool wantEarliestPass,
      ref FilteredSinglePassInfo filteredPassInfo,
      object /* IProfileCell*/ profileCell,
      bool performAttributeSubFilter,
      ICellPassAttributeFilterProcessingAnnex filterAnnex
      );

    /// <summary>
    /// Creates a fingerprint of the attribute filter based on the content of the filter that
    /// has a differentiated bearing on the use of cached general sub grid results
    /// </summary>
    /// <returns></returns>
    string SpatialCacheFingerprint();
  }
}
