using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.TAGFiles.Models.Classes.Integrator
{
  public interface IGroupedSubGridTreeIntegrator
  {
    IServerSubGridTree[] Trees { get; set; }

    IServerSubGridTree IntegrateSubGridTreeGroup();
  }
}
