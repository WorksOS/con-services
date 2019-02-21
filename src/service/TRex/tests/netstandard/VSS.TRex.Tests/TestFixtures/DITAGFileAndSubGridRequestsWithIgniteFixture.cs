using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.DI;
using VSS.TRex.Exceptions;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.Exports.Surfaces.Executors.Tasks;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Factories;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.Reports.Gridded.Executors.Tasks;
using VSS.TRex.SubGrids.GridFabric.Listeners;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DITAGFileAndSubGridRequestsWithIgniteFixture : DITAGFileAndSubGridRequestsFixture
  {
    public DITAGFileAndSubGridRequestsWithIgniteFixture() : base()
    {
      SetupFixture();
    }

    private static ISubGridPipelineBase SubGridPipelineFactoryMethod(PipelineProcessorPipelineStyle key)
    {
      switch (key)
      {
        case PipelineProcessorPipelineStyle.DefaultAggregative:
          return new SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse>();
        case PipelineProcessorPipelineStyle.DefaultProgressive:
          return new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>();
        default:
          return null;
      }
    }

    private static ITRexTask SubGridTaskFactoryMethod(PipelineProcessorTaskStyle key)
    {
      switch (key)
      {
        case PipelineProcessorTaskStyle.AggregatedPipelined:
          return new AggregatedPipelinedSubGridTask();
        case PipelineProcessorTaskStyle.PatchExport:
          return new PatchTask();
        case PipelineProcessorTaskStyle.SurfaceExport:
          return new SurfaceTask();
        case PipelineProcessorTaskStyle.GriddedReport:
          return new GriddedReportTask();
        case PipelineProcessorTaskStyle.PVMRendering:
          return new PVMRenderingTask();
        case PipelineProcessorTaskStyle.CSVExport:
          return new CSVExportTask();

        default:
          return null;
      }
    }

    public new void SetupFixture()
    {
      // Wire up Ignite Compute Apply and Broadcast apis on the Compute interface
      var mockCompute = new Mock<ICompute>();

      // Pretend there is a single node in the cluster group
      var mockClusterNode = new Mock<IClusterNode>();
      mockClusterNode.Setup(x => x.GetAttribute<string>("TRexNodeId")).Returns("UnitTest-TRexNodeId");

      var mockClusterNodes = new Mock<ICollection<IClusterNode>>();
      mockClusterNodes.Setup(x => x.Count).Returns(1);

      var messagingDictionary = new Dictionary<object, object>(); // topic => listener
     
      var mockMessaging = new Mock<IMessaging>();
      mockMessaging.Setup(x => x.LocalListen(It.IsAny<IMessageListener<byte[]>>(), It.IsAny<object>())).Callback((IMessageListener<byte[]> listener, object topic) =>
      {
        messagingDictionary.Add(topic, listener);
      });
      mockMessaging.Setup(x => x.Send(It.IsAny<object>(), It.IsAny<object>())).Callback((object message, object topic) =>
      {
        messagingDictionary.TryGetValue(topic, out var listener);
        if (listener is SubGridListener _listener)
         _listener.Invoke(Guid.Empty, message as byte[]);
        else
          throw new TRexException($"Type of listener ({listener}) not SubGridListener as expected.");
      });


      var mockClusterGroup = new Mock<IClusterGroup>();
      mockClusterGroup.Setup(x => x.GetNodes()).Returns(mockClusterNodes.Object);
      mockClusterGroup.Setup(x => x.GetCompute()).Returns(mockCompute.Object);
      mockClusterGroup.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);

      mockCompute.Setup(x => x.ClusterGroup).Returns(mockClusterGroup.Object);

      var mockCluster = new Mock<ICluster>();
      mockCluster.Setup(x => x.ForAttribute(It.IsAny<string>(), It.IsAny<string>())).Returns(mockClusterGroup.Object);
      mockCluster.Setup(x => x.GetLocalNode()).Returns(mockClusterNode.Object);

      var mockIgnite = new Mock<IIgnite>();
      mockIgnite.Setup(x => x.GetCluster()).Returns(mockCluster.Object);

      DIBuilder
        .Continue()
        .Add(x => x.AddTransient<Func<string, IIgnite>>(factory => gridName => mockIgnite.Object))
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Add(x => x.AddSingleton<IPipelineProcessorFactory>(new PipelineProcessorFactory()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>(provider => SubGridPipelineFactoryMethod))
        .Add(x => x.AddSingleton<Func<PipelineProcessorTaskStyle, ITRexTask>>(provider => SubGridTaskFactoryMethod))
        .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))
        .Add(x => x.AddSingleton(mockCompute))
        .Complete();
    }

    public static void AddApplicationGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TArgument, TResponse>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();
      mockCompute.Setup(x => x.Apply(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) => func.Invoke(argument));
      mockCompute.Setup(x => x.ApplyAsync(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        var task = new Task<TResponse>(() => func.Invoke(argument));
        task.Start();
        return task;
      });
    }

    public static void AddClusterComputeGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TArgument, TResponse>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();
      mockCompute.Setup(x => x.Broadcast(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) => new List<TResponse> { func.Invoke(argument) });
      mockCompute.Setup(x => x.BroadcastAsync(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        var task = new Task<ICollection<TResponse>>(() => new List<TResponse>{ func.Invoke(argument) });
        task.Start();
        return task; 
      });
    }
  }
}


