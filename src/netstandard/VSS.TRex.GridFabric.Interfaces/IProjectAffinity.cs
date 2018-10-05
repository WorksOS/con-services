using System;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface IProjectAffinity
  {
    /// <summary>
    /// The GUID for the project the subgrid data belongs to.
    /// </summary>
    Guid ProjectUID { get; set; }
  }
}
