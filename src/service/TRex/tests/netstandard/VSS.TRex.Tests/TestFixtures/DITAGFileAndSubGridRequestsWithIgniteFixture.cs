using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Messaging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Factories;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exceptions;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.Exports.Surfaces.Executors.Tasks;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Factories;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.Reports.Gridded.Executors.Tasks;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.SubGrids.GridFabric.Listeners;
using VSS.TRex.SubGridTrees.Interfaces;

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

      // Set up the Ignite message fabric mocks to plumb sender and receiver together
      var messagingDictionary = new Dictionary<object, object>(); // topic => listener
     
      var mockMessaging = new Mock<IMessaging>();
      mockMessaging
        .Setup(x => x.LocalListen(It.IsAny<IMessageListener<byte[]>>(), It.IsAny<object>()))
        .Callback((IMessageListener<byte[]> listener, object topic) =>
      {
        messagingDictionary.Add(topic, listener);
      });

      mockMessaging
        .Setup(x => x.LocalListen(It.IsAny<IMessageListener<ISiteModelAttributesChangedEvent>>(), It.IsAny<object>()))
        .Callback((IMessageListener<ISiteModelAttributesChangedEvent> listener, object topic) =>
        {
          messagingDictionary.Add(topic, listener);
        });

      mockMessaging
        .Setup(x => x.Send(It.IsAny<object>(), It.IsAny<object>()))
        .Callback((object message, object topic) =>
      {
        messagingDictionary.TryGetValue(topic, out var listener);
        if (listener is SubGridListener _listener)
         _listener.Invoke(Guid.Empty, message as byte[]);
        else
          throw new TRexException($"Type of listener ({listener}) not SubGridListener as expected.");
      });

      mockMessaging
        .Setup(x => x.SendOrdered(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()))
        .Callback((object message, object topic, TimeSpan? timeSpan) =>
      {
        messagingDictionary.TryGetValue(topic, out var listener);
        if (listener is SubGridListener _listener1)
          _listener1.Invoke(Guid.Empty, message as byte[]);
        else if (listener is SiteModelAttributesChangedEventListener _listener2)
          _listener2.Invoke(Guid.Empty, message as SiteModelAttributesChangedEvent);
        else
          throw new TRexException($"Type of listener not SubGridListener or SiteModelAttributesChangedEventListener as expected.");
      });

      var mockClusterGroup = new Mock<IClusterGroup>();
      mockClusterGroup.Setup(x => x.GetNodes()).Returns(mockClusterNodes.Object);
      mockClusterGroup.Setup(x => x.GetCompute()).Returns(mockCompute.Object);
      mockClusterGroup.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);

      mockCompute.Setup(x => x.ClusterGroup).Returns(mockClusterGroup.Object);

      var mockCluster = new Mock<ICluster>();
      mockCluster.Setup(x => x.ForAttribute(It.IsAny<string>(), It.IsAny<string>())).Returns(mockClusterGroup.Object);
      mockCluster.Setup(x => x.GetLocalNode()).Returns(mockClusterNode.Object);
      mockCluster.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);

      var mockIgnite = new Mock<IIgnite>();
      mockIgnite.Setup(x => x.GetCluster()).Returns(mockCluster.Object);
      mockIgnite.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);

      DIBuilder
        .Continue()
        .Add(x => x.AddTransient<Func<string, IIgnite>>(factory => gridName => mockIgnite.Object))
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Add(x => x.AddSingleton<IPipelineProcessorFactory>(new PipelineProcessorFactory()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>(provider => SubGridPipelineFactoryMethod))
        .Add(x => x.AddSingleton<Func<PipelineProcessorTaskStyle, ITRexTask>>(provider => SubGridTaskFactoryMethod))
        .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))

        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventSender>(new SiteModelAttributesChangedEventSender()))
        // Register the listener for site model attribute change notifications
        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventListener>(new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName())))
        .Add(x => x.AddSingleton<IDesignFiles>(new DesignFiles()))
        .Add(x => x.AddSingleton<IOptimisedTTMProfilerFactory>(new OptimisedTTMProfilerFactory()))

        .Add(x => x.AddSingleton(mockCompute))
        .Add(x => x.AddSingleton(mockIgnite))
        .Complete();

      DIContext.Obtain<ISiteModelAttributesChangedEventListener>().StartListening();
      ResetDynamicMockedIgniteContent();
    }

    public static void ResetDynamicMockedIgniteContent()
    {
      // Create the dictionary to contain all the mocked caches
      var cacheDictionary = new Dictionary<string, object>(); // object = ICache<TK, TV>

      // Create he mocked cache for the existence maps cache and any other cache using this signature
      var mockIgnite = DIContext.Obtain<Mock<IIgnite>>();

      mockIgnite.Setup(x => x.GetOrCreateCache<INonSpatialAffinityKey, byte[]>(It.IsAny<CacheConfiguration>())).Returns((CacheConfiguration cfg) =>
      {
        if (cacheDictionary.TryGetValue(cfg.Name, out var cache))
          return (ICache<INonSpatialAffinityKey, byte[]>)cache;

        var mockCache = new Mock<ICache<INonSpatialAffinityKey, byte[]>>();
        var mockCacheDictionary = new Dictionary<INonSpatialAffinityKey, byte[]>();

        mockCache.Setup(x => x.Get(It.IsAny<INonSpatialAffinityKey>())).Returns((INonSpatialAffinityKey key) =>
        {
          if (mockCacheDictionary.TryGetValue(key, out var value))
            return value;
          throw new KeyNotFoundException($"Key {key} not found in mock cache");
        });

        mockCache.Setup(x => x.Put(It.IsAny<INonSpatialAffinityKey>(), It.IsAny<byte[]>())).Callback((INonSpatialAffinityKey key, byte[] value) =>
        {
          mockCacheDictionary.Add(key, value);
        });

        cacheDictionary.Add(cfg.Name, mockCache.Object);
        return mockCache.Object;
      });
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

    public static Guid AddDesignToSiteModel(ref ISiteModel siteModel, string filePath, string fileName)
    {
      var filePathAndName = Path.Combine(filePath, fileName);

      TTMDesign ttm = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
      var designLoadResult = ttm.LoadFromFile(filePathAndName, false); 
      designLoadResult.Should().Be(DesignLoadResult.Success);

      BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
      ttm.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
      ttm.GetHeightRange(out extents.MinZ, out extents.MaxZ);

      Guid designUid = Guid.NewGuid();
      var existenceMaps = DIContext.Obtain<IExistenceMaps>();

      // Create the design surface in the site model
      var designSurface = DIContext.Obtain<IDesignManager>().Add(siteModel.ID,
        new DesignDescriptor(designUid, filePath, fileName, 0), extents);
      existenceMaps.SetExistenceMap(siteModel.ID, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, designSurface.ID, ttm.SubGridOverlayIndex());

      // get the newly updated site model with the design reference included
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID);

      // Place the design into the project temp folder prior to executing the render so the design profiler
      // will not attempt to access the file from S3
      var tempPath = DesignHelper.EstablishLocalDesignFilepath(siteModel.ID);
      var srcFileName = Path.Combine(filePath, fileName);
      var destFileName = Path.Combine(tempPath, fileName);

      File.Copy(srcFileName, destFileName);
      File.Copy(srcFileName + Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION,
                destFileName + Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION);
      File.Copy(srcFileName + Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION,
                destFileName + Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION);

      return designUid;
    }
  }
}


