using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Files;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Files.DXF;
using VSS.TRex.Gateway.Common.Executors.Files;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Files
{
  public class ExtractDXFBoundariesExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    public ExtractDXFBoundariesExecutorTests()
    {
      // Mock the coordinate conversion service
      var mockCoordinateService = new Mock<IConvertCoordinates>();
      mockCoordinateService.Setup(x => x.NEEToLLH(It.IsAny<string>(), It.IsAny<XYZ[]>())).Returns((string csib, XYZ[] coordinates) => Task.FromResult((RequestErrorStatus.OK, coordinates)));

      DIBuilder.Continue().Add(x => x.AddSingleton(mockCoordinateService.Object)).Complete();
    }

    [Fact]
    public void Creation()
    {
      var executor = new ExtractDXFBoundariesExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IServiceExceptionHandler>());
      executor.Should().NotBeNull();
    }

    [Fact]
    public void Fail_With_FileNotExist()
    {
      var request = new DXFBoundariesRequest("", ImportedFileType.SiteBoundary, Path.Combine("TestData", "does-not-exist.dxf"), DxfUnitsType.Meters, 10);
      var executor = new ExtractDXFBoundariesExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IServiceExceptionHandler>());
      executor.Should().NotBeNull();
      
      Func<Task> act = async () => await executor.ProcessAsync(request);
      act.Should().Throw<ServiceException>().WithMessage("File*does not exist");
    }

    [Theory]
    [InlineData("Southern Motorway 55 point polygon.dxf", DxfUnitsType.Meters, 1, 1001, "55 points", DXFLineWorkBoundaryType.GenericBoundary)]
    [InlineData("avoidMeBoundary.dxf", DxfUnitsType.Meters, 1, 12, "avoidMeBoundary", DXFLineWorkBoundaryType.AvoidanceZone)]
    [InlineData("Southern Motorway Site Boundaries.dxf", DxfUnitsType.Meters, 7, 4, "Fill", DXFLineWorkBoundaryType.Stockpile)]
    [InlineData("100_sided_giraffe.dxf", DxfUnitsType.Meters, 1, 104, "stockpile_100_sides", DXFLineWorkBoundaryType.Stockpile)]
    [InlineData("vssBoundary.dxf", DxfUnitsType.Meters, 1, 101, "vssBoundary", DXFLineWorkBoundaryType.GenericBoundary)]

    public async void ASCII_DXF_Boundaries_UnderLimit(string fileName, DxfUnitsType units, int expectedBoundaryCount, int firstBoundaryVertexCount, string expectedName, DXFLineWorkBoundaryType expectedType)
    {
      var request = new DXFBoundariesRequest("", ImportedFileType.SiteBoundary, Path.Combine("TestData", fileName), units, 10);
      var executor = new ExtractDXFBoundariesExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IServiceExceptionHandler>());
      executor.Should().NotBeNull();

      var result = await executor.ProcessAsync(request);
      result.Should().NotBeNull();
      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.Message.Should().Be("Success");

      if (result is DXFBoundaryResult boundary)
      {
        boundary.Boundaries.Count.Should().Be(expectedBoundaryCount);
        boundary.Boundaries[0].Fence.Count.Should().Be(firstBoundaryVertexCount);
        boundary.Boundaries[0].Name.Should().Be(expectedName);
        boundary.Boundaries[0].Type.Should().Be(expectedType);
      }
      else
      {
        false.Should().BeTrue(); // fail the test
      }
    }

    [Theory]
    [InlineData("11-12_Binary.dxf", DxfUnitsType.Meters, 10, 1, "11-12_Binary1", DXFLineWorkBoundaryType.GenericBoundary)]
    [InlineData("Binary lesson-11.dxf", DxfUnitsType.Meters, 8, 2, "Binary lesson-111", DXFLineWorkBoundaryType.GenericBoundary)]
    [InlineData("Binary lesson-3.dxf", DxfUnitsType.Meters, 0, 0, "-1", DXFLineWorkBoundaryType.Unknown)]
    public async void Binary_DXF_Boundaries_UnderLimit(string fileName, DxfUnitsType units, int expectedBoundaryCount, int firstBoundaryVertexCount, string expectedName, DXFLineWorkBoundaryType expectedType)
    {
      var request = new DXFBoundariesRequest("", ImportedFileType.SiteBoundary, Path.Combine("TestData", fileName), units, 10);
      var executor = new ExtractDXFBoundariesExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IServiceExceptionHandler>());
      executor.Should().NotBeNull();

      var result = await executor.ProcessAsync(request);
      result.Should().NotBeNull();
      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.Message.Should().Be("Success");

      if (result is DXFBoundaryResult boundary)
      {
        boundary.Boundaries.Count.Should().Be(expectedBoundaryCount);

        if (expectedBoundaryCount > 0)
        {
          boundary.Boundaries[0].Fence.Count.Should().Be(firstBoundaryVertexCount);
          boundary.Boundaries[0].Name.Should().Be(expectedName);
          boundary.Boundaries[0].Type.Should().Be(expectedType);
        }
      }
      else
      {
        false.Should().BeTrue(); // fail the test
      }
    }

    [Theory]
    [InlineData("Southern Motorway Site Boundaries.dxf", DxfUnitsType.Meters, 4)]
    public async void Boundaries_OverLimit(string fileName, DxfUnitsType units, int firstBoundaryVertexCount)
    {
      const int LIMIT = 5;

      var request = new DXFBoundariesRequest("", ImportedFileType.SiteBoundary, Path.Combine("TestData", fileName), units, LIMIT);
      var executor = new ExtractDXFBoundariesExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IServiceExceptionHandler>());
      executor.Should().NotBeNull();

      var result = await executor.ProcessAsync(request);
      result.Should().NotBeNull();
      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.Message.Should().Be("Success");

      if (result is DXFBoundaryResult boundary)
      {
        boundary.Boundaries.Count.Should().Be(LIMIT);
        boundary.Boundaries[0].Fence.Count.Should().Be(firstBoundaryVertexCount);
      }
      else
      {
        false.Should().BeTrue(); // fail the test
      }
    }

    [Fact]
    public async void Fail_With_InvalidFile()
    {
      var request = new DXFBoundariesRequest("", ImportedFileType.SiteBoundary, Path.Combine("TestData", "TransferTestDesign.ttm"), DxfUnitsType.Meters, 10);
      var executor = new ExtractDXFBoundariesExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IServiceExceptionHandler>());
      executor.Should().NotBeNull();

      Func<Task> act = async () => await executor.ProcessAsync(request);
      act.Should().Throw<ServiceException>().WithMessage($"Error processing file: {DXFUtilitiesResult.UnknownFileFormat}");
    }
  }
}
