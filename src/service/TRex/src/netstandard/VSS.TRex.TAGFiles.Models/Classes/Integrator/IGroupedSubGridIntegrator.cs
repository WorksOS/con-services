using System;
using System.Collections.Generic;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.TAGFiles.Models.Classes.Integrator
{
  public interface IGroupedSubGridIntegrator
  {
    List<(IServerSubGridTree, DateTime, DateTime)> Trees { get; set; }

    void IntegrateSubGridGroup(IServerLeafSubGrid resultSubGrid);
  }
}
