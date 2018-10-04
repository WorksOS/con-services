using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  public class SiteModelMachineDesignList : List<string>, ISiteModelMachineDesignList
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);


    /// <summary>
    /// Indexer supporting locating designs by the design name
    /// </summary>
    /// <param name="designName"></param>
    /// <returns></returns>
    public string this[string designName]
    {
      get
      {
        int index = IndexOf(designName);
        return index > 0 ? designName : string.Empty;
      }
    }

    //public int IndexOf(string designName) => FindIndex(x => x == designName);

    public int CreateNew(string name)
    {
      int index = IndexOf(name);

      if (index != -1)
      {
        Log.LogError($"An identical machine design ({name}) already exists in the machine designs for this site.");
        return index;
      }

      Add(name);
      return IndexOf(name);
    }

  }
}
