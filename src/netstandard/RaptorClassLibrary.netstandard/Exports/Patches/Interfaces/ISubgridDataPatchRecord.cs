using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Exports.Patches.Interfaces
{
  public interface ISubgridDataPatchRecord
  {
    void Populate(IClientLeafSubGrid subGrid);
  }
}
