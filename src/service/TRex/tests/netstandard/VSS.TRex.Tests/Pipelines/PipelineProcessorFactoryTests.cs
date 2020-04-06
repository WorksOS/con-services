using System;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Factories;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Pipelines
{
  public class PipelineProcessorFactoryTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var factory = new PipelineProcessorFactory();

      factory.Should().NotBeNull();
    }

    [Fact]
    public void NewInstance_NoBuild()
    {
      var factory = new PipelineProcessorFactory();

      var processor = factory.NewInstanceNoBuild<SubGridsRequestArgument>(
        Guid.NewGuid(), 
        Guid.NewGuid(), 
        GridDataType.Height,
        new PatchRequestResponse(), 
        new FilterSet(new CombinedFilter()), 
        new DesignOffset(), 
        DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.PatchExport),
        DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
        DIContext.Obtain<IRequestAnalyser>(),
        false,
        false,
        BoundingIntegerExtent2D.Inverted(),
        new LiftParameters());

      processor.Should().NotBeNull();
    }

    [Fact]
    public async Task NewInstance()
    {
      var factory = new PipelineProcessorFactory();

      var processor = await factory.NewInstance<SubGridsRequestArgument>(
        Guid.NewGuid(),
        Guid.NewGuid(),
        GridDataType.Height,
        new PatchRequestResponse(),
        new FilterSet(new CombinedFilter()),
        new DesignOffset(), 
        DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.PatchExport),
        DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
        DIContext.Obtain<IRequestAnalyser>(),
        false,
        false,
        BoundingIntegerExtent2D.Inverted(),
        new LiftParameters());

      processor.Should().BeNull("because there is no site model");

      // Configure the request analyser to return a single page of results.
//      processor.RequestAnalyser.SinglePageRequestNumber = 1;
//      processor.RequestAnalyser.SinglePageRequestSize = 10;
//      processor.RequestAnalyser.SubmitSinglePageOfRequests = true;

     // processor.Build().Should().BeFalse("because there is no sitemodel");
    }
  }
}
