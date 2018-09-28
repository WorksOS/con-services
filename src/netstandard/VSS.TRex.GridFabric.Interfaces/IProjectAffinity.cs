using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface IProjectAffinity
  {
    /// <summary>
    /// A numeric ID for the project the subgrid data belongs to.
    /// </summary>
    Guid ProjectID { get; set; }
  }
}
