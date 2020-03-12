using Apache.Ignite.Core.Compute;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Analytics.Common
{
  public class BaseTests<TArgument, TResponse> : IClassFixture<DILoggingFixture>
  {
    protected void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<IComputeFunc<TArgument, TResponse>, TArgument, TResponse>();

    protected void AddClusterComputeGridRouting()
    {
      IgniteMock.AddClusterComputeGridRouting<IComputeFunc<TArgument, TResponse>, TArgument, TResponse>();
      IgniteMock.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }
  }
}
