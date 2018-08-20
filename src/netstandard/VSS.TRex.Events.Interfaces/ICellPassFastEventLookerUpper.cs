using VSS.TRex.Filters.Models;
using VSS.TRex.Filters.Interfaces;
//using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Events.Interfaces
{
  public interface ICellPassFastEventLookerUpper
  {
//    ISiteModel SiteModel { get; set; }

    /// <summary>
    /// Initialise tracking state values to null
    /// </summary>
    void ClearLastValues();

    void PopulateFilteredValues(FilteredPassData[] passes,
      int firstPassIndex, int lastPassIndex,
      IFilteredValuePopulationControl populationControl,
      bool ignoreBussinessRulesRules);
  }
}
