using System;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Factories;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
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

      var processor = factory.NewInstanceNoBuild(requestDescriptor: Guid.NewGuid(), 
        dataModelID: Guid.NewGuid(), 
        gridDataType: GridDataType.Height,
        response: new PatchRequestResponse(), 
        filters: new FilterSet(new CombinedFilter()), 
        cutFillDesign: new DesignOffset(), 
        task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.PatchExport),
        pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
        requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
        requireSurveyedSurfaceInformation: false,
        requestRequiresAccessToDesignFileExistenceMap: false,
        overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted());

      processor.Should().NotBeNull();
    }

    [Fact]
    public async Task NewInstance()
    {
      var factory = new PipelineProcessorFactory();

      var processor = await factory.NewInstance(requestDescriptor: Guid.NewGuid(),
        dataModelID: Guid.NewGuid(),
        gridDataType: GridDataType.Height,
        response: new PatchRequestResponse(),
        filters: new FilterSet(new CombinedFilter()),
        cutFillDesign: new DesignOffset(), 
        task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.PatchExport),
        pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
        requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
        requireSurveyedSurfaceInformation: false,
        requestRequiresAccessToDesignFileExistenceMap: false,
        overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted());

      processor.Should().BeNull("because there is no site model");

      // Configure the request analyser to return a single page of results.
//      processor.RequestAnalyser.SinglePageRequestNumber = 1;
//      processor.RequestAnalyser.SinglePageRequestSize = 10;
//      processor.RequestAnalyser.SubmitSinglePageOfRequests = true;

     // processor.Build().Should().BeFalse("because there is no sitemodel");
    }
  }
}
