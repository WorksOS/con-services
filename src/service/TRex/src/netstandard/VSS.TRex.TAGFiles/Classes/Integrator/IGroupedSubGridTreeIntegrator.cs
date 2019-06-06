using System;
using System.Collections.Generic;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
  public interface IGroupedSubGridTreeIntegrator
  {
    List<(IServerSubGridTree, DateTime, DateTime)> Trees { get; set; }

    IServerSubGridTree IntegrateSubGridTreeGroup();
  }
}
