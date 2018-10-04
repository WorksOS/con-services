using System.Collections.Generic;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelMachineDesignList : IList<string>
  {
    int CreateNew(string name);

    //int IndexOf(string designName);

    /// <summary>
    /// Indexer supporting locating machine designs by the name
    /// </summary>
    /// <param name="designName"></param>
    /// <returns></returns>
    string this[string designName] { get; }
  }
}
