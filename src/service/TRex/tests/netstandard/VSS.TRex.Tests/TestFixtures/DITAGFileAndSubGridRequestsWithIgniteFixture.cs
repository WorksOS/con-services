using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DITAGFileAndSubGridRequestsWithIgniteFixture : DITAGFileAndSubGridRequestsFixture
  {
    public DITAGFileAndSubGridRequestsWithIgniteFixture() : base()
    {
      SetupFixture();
    }

    public new void SetupFixture()
    {
      // Wire up Ignite Compute Apply and Broadcast apis on the Compute interface
      var mockCompute = new Mock<ICompute>();

      //mockCompute.Setup(x => x.Apply(It.IsAny<IComputeFunc<BaseRequestArgument, BaseRequestResponse>>(), It.IsAny<BaseRequestArgument>())).Returns((IComputeFunc<BaseRequestArgument, BaseRequestResponse> func, BaseRequestArgument argument) => func.Invoke(argument));
      //mockCompute.Setup(x => x.Broadcast(It.IsAny<IComputeFunc<BaseRequestArgument, BaseRequestResponse>>(), It.IsAny<BaseRequestArgument>())).Returns((IComputeFunc<BaseRequestArgument, BaseRequestResponse> func, BaseRequestArgument argument) => new List<BaseRequestResponse> { func.Invoke(argument) });

      //mockCompute.Setup(x => x.Apply(It.IsAny<IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesRequestArgument>>(), It.IsAny<SimpleVolumesRequestArgument>())).Returns((IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesRequestArgument> func, SimpleVolumesRequestArgument argument) => func.Invoke(argument));
      //mockCompute.Setup(x => x.Broadcast(It.IsAny<IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesRequestArgument>>(), It.IsAny<SimpleVolumesRequestArgument>())).Returns((IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesRequestArgument> func, SimpleVolumesRequestArgument argument) => new List<SimpleVolumesRequestResponse> { func.Invoke(argument) });

      //mockCompute.Setup(x => x.Apply(It.IsAny<BaseComputeFunc>(), It.IsAny<BaseRequestArgument>())).Returns((BaseComputeFunc func, BaseRequestArgument argument) => func.Invoke(argument));
      //mockCompute.Setup(x => x.Broadcast(It.IsAny<BaseComputeFunc>(), It.IsAny<BaseRequestArgument>())).Returns(BaseComputeFunc func, BaseRequestArgument argument) => new List<BaseRequestResponse> { func.Invoke(argument) });

      // Pretend there is a single node in the cluster group
      var mockClusterNodes = new Mock<ICollection<IClusterNode>>();
      mockClusterNodes.Setup(x => x.Count).Returns(1);

      var mockClusterGroup = new Mock<IClusterGroup>();
      mockClusterGroup.Setup(x => x.GetNodes()).Returns(mockClusterNodes.Object);
      mockClusterGroup.Setup(x => x.GetCompute()).Returns(mockCompute.Object);
      mockCompute.Setup(x => x.ClusterGroup).Returns(mockClusterGroup.Object);

      var mockCluster = new Mock<ICluster>();
      mockCluster.Setup(x => x.ForAttribute(It.IsAny<string>(), It.IsAny<string>())).Returns(mockClusterGroup.Object);

      var mockIgnite = new Mock<IIgnite>();
      mockIgnite.Setup(x => x.GetCluster()).Returns(mockCluster.Object);

      DIBuilder
        .Continue()
        .Add(x => x.AddTransient<Func<string, IIgnite>>(factory => gridName => mockIgnite.Object))
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))
        .Add(x => x.AddSingleton(mockCompute))
        .Complete();
    }

    public static void AddApplicationGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TArgument, TResponse>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();
      mockCompute.Setup(x => x.Apply(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) => func.Invoke(argument));
      mockCompute.Setup(x => x.ApplyAsync(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) => new Task<TResponse>(() => func.Invoke(argument)));
    }

    public static void AddClusterComputeGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TArgument, TResponse>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();
      mockCompute.Setup(x => x.Broadcast(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) => new List<TResponse> { func.Invoke(argument) });
      mockCompute.Setup(x => x.BroadcastAsync(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) => new Task<ICollection<TResponse>>(() => new List<TResponse> {func.Invoke(argument)}));
    }
  }
}


