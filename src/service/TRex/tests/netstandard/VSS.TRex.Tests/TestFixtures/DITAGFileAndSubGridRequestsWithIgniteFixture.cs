using System;
using System.IO;
using System.Linq;
using Apache.Ignite.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Factories;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL;
using VSS.TRex.Designs.TTM;
using VSS.TRex.DI;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.Exports.Surfaces.Executors.Tasks;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Grids;
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
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using Consts = VSS.TRex.ExistenceMaps.Interfaces.Consts;
using VSS.TRex.QuantizedMesh.Executors.Tasks;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.Volumes.Executors.Tasks;
using VSS.TRex.Volumes.GridFabric.Arguments;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DITAGFileAndSubGridRequestsWithIgniteFixture : DITAGFileAndSubGridRequestsFixture, IDisposable
  {
    public DITAGFileAndSubGridRequestsWithIgniteFixture()
    {
      SetupFixture();
    }

    private static ISubGridPipelineBase SubGridPipelineFactoryMethod(PipelineProcessorPipelineStyle key)
    {
      return key switch
      {
        PipelineProcessorPipelineStyle.DefaultAggregative => new SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse>(),
        PipelineProcessorPipelineStyle.DefaultProgressive => new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(),
        PipelineProcessorPipelineStyle.ProgressiveVolumes => new SubGridPipelineProgressive<ProgressiveVolumesSubGridsRequestArgument, SubGridRequestsResponse>(),
        _ => null
      };
    }

    private static ITRexTask SubGridTaskFactoryMethod(PipelineProcessorTaskStyle key)
    {
      return key switch
      {
        PipelineProcessorTaskStyle.AggregatedPipelined => new AggregatedPipelinedSubGridTask(),
        PipelineProcessorTaskStyle.PatchExport => new PatchTask(),
        PipelineProcessorTaskStyle.SurfaceExport => new SurfaceTask(),
        PipelineProcessorTaskStyle.GriddedReport => new GriddedReportTask(),
        PipelineProcessorTaskStyle.PVMRendering => new PVMRenderingTask(),
        PipelineProcessorTaskStyle.CSVExport => new CSVExportTask(),
        PipelineProcessorTaskStyle.QuantizedMesh => new QuantizedMeshTask(),
        PipelineProcessorTaskStyle.SimpleVolumes => new VolumesComputationTask(),
        PipelineProcessorTaskStyle.ProgressiveVolumes => new VolumesComputationTask(),

        _ => null
      };
    }

    public new void SetupFixture()
    {
      var igniteMock = new IgniteMock();

      DIBuilder
        .Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => igniteMock.mockIgnite.Object))
        .Add(x => x.AddSingleton<IPipelineProcessorFactory>(new PipelineProcessorFactory()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>(provider => SubGridPipelineFactoryMethod))
        .Add(x => x.AddSingleton<Func<PipelineProcessorTaskStyle, ITRexTask>>(provider => SubGridTaskFactoryMethod))
        .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))

        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventSender>(new SiteModelAttributesChangedEventSender()))
        // Register the listener for site model attribute change notifications
        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventListener>(new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName())))
        .Add(x => x.AddSingleton<IDesignFiles>(new DesignFiles()))
        .Add(x => x.AddSingleton<IOptimisedTTMProfilerFactory>(new OptimisedTTMProfilerFactory()))
        .Add(x => x.AddSingleton<IDesignClassFactory>(new DesignClassFactory()))
        .Add(x => x.AddSingleton<IConvertCoordinates>(new ConvertCoordinates()))
        .Add(x => x.AddSingleton(igniteMock.mockCompute))
        .Add(x => x.AddSingleton(igniteMock.mockIgnite))
        .Complete();

      // Start the 'mocked' listener
      DIContext.Obtain<ISiteModelAttributesChangedEventListener>().StartListening();

      IgniteMock.ResetDynamicMockedIgniteContent();
    }

    /// <summary>
    /// Adds a design identified by a filename and location to the site model
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="constructIndexFilesOnLoad"></param>
    /// <returns></returns>
    public static Guid AddDesignToSiteModel(ref ISiteModel siteModel, string filePath, string fileName,
      bool constructIndexFilesOnLoad)
    {
      var filePathAndName = Path.Combine(filePath, fileName);

      var ttm = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
      var designLoadResult = ttm.LoadFromFile(filePathAndName, constructIndexFilesOnLoad); 
      designLoadResult.Should().Be(DesignLoadResult.Success);

      var extents = new BoundingWorldExtent3D();
      ttm.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
      ttm.GetHeightRange(out extents.MinZ, out extents.MaxZ);

      var designUid = Guid.NewGuid();
      var existenceMaps = DIContext.Obtain<IExistenceMaps>();

      // Create the design surface in the site model
      var designSurface = DIContext.Obtain<IDesignManager>().Add(siteModel.ID,
        new DesignDescriptor(designUid, filePath, fileName), extents);
      existenceMaps.SetExistenceMap(siteModel.ID, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, designSurface.ID, ttm.SubGridOverlayIndex());

      // get the newly updated site model with the design reference included
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID);

      // Place the design into the project temp folder prior to executing the render so the design profiler
      // will not attempt to access the file from S3
      var tempPath = FilePathHelper.GetTempFolderForProject(siteModel.ID);
      var srcFileName = Path.Combine(filePath, fileName);
      var destFileName = Path.Combine(tempPath, fileName);

      File.Copy(srcFileName, destFileName);
      File.Copy(srcFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION,
                destFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION);
      File.Copy(srcFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION,
                destFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION);
      File.Copy(srcFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_BOUNDARY_FILE_EXTENSION,
        destFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_BOUNDARY_FILE_EXTENSION);

      return designUid;
    }

    /// <summary>
    /// Adds a design identified by a filename and location to the site model
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="constructIndexFilesOnLoad"></param>
    /// <returns></returns>
    public static Guid AddSVLAlignmentDesignToSiteModel(ref ISiteModel siteModel, string filePath, string fileName,
      bool constructIndexFilesOnLoad)
    {
      var filePathAndName = Path.Combine(filePath, fileName);

      var svl = NFFFile.CreateFromFile(filePathAndName);
      var designLoadResult = svl.LoadFromFile(filePathAndName);
      designLoadResult.Should().Be(true);

      var masterAlignment = svl.GuidanceAlignments.FirstOrDefault(x => x.IsMasterAlignment());
      masterAlignment.Should().NotBeNull();

      var designUid = Guid.NewGuid();

      // Create the design surface in the site model
      /*var alignmentDesign = */ DIContext.Obtain<IDesignManager>().Add(siteModel.ID, new DesignDescriptor(designUid, filePath, fileName), masterAlignment.BoundingBox());

      // get the newly updated site model with the design reference included
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID);

      // Place the design into the project temp folder prior to executing the render so the design profiler
      // will not attempt to access the file from S3
      var tempPath = FilePathHelper.GetTempFolderForProject(siteModel.ID);
      var srcFileName = Path.Combine(filePath, fileName);
      var destFileName = Path.Combine(tempPath, fileName);

      File.Copy(srcFileName, destFileName);

      return designUid;
    }

    /// <summary>
    /// Adds a surveyed surface identified by a file name and location, plus asAtDate.
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="asAtDate"></param>
    /// <param name="constructIndexFilesOnLoad"></param>
    /// <returns></returns>
    public static Guid AddSurveyedSurfaceToSiteModel(ref ISiteModel siteModel, string filePath, string fileName, DateTime asAtDate,
     bool constructIndexFilesOnLoad)
    {
      var filePathAndName = Path.Combine(filePath, fileName);

      TTMDesign ttm = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
      var loadResult = ttm.LoadFromFile(filePathAndName, constructIndexFilesOnLoad);
      loadResult.Should().Be(DesignLoadResult.Success);

      BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
      ttm.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
      ttm.GetHeightRange(out extents.MinZ, out extents.MaxZ);

      Guid surveyedSurfaceUid = Guid.NewGuid();
      var existenceMaps = DIContext.Obtain<IExistenceMaps>();

      // Create the design surface in the site model
      var surveyedSurface = DIContext.Obtain<ISurveyedSurfaceManager>().Add(siteModel.ID,
        new DesignDescriptor(surveyedSurfaceUid, filePath, fileName), asAtDate, extents);
      existenceMaps.SetExistenceMap(siteModel.ID, Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, surveyedSurface.ID, ttm.SubGridOverlayIndex());

      // get the newly updated site model with the surveyed surface included
      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID);

      // Place the surveyed surface into the project temp folder prior to executing the render so the design profiler
      // will not attempt to access the file from S3
      var tempPath = FilePathHelper.GetTempFolderForProject(siteModel.ID);
      var srcFileName = Path.Combine(filePath, fileName);
      var destFileName = Path.Combine(tempPath, fileName);

      File.Copy(srcFileName, destFileName);
      File.Copy(srcFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION,
                destFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION);
      File.Copy(srcFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION,
                destFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION);
      File.Copy(srcFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_BOUNDARY_FILE_EXTENSION,
        destFileName + TRex.Designs.TTM.Optimised.Consts.DESIGN_BOUNDARY_FILE_EXTENSION);

      return surveyedSurfaceUid;
    }


    /// <summary>
    /// Creates a new otherwise empty site model configured to provide mutable data representation and adds
    /// an initial bulldozer machine to it
    /// </summary>
    /// <returns></returns>
    public static ISiteModel NewEmptyModel()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      // Switch to mutable storage representation to allow creation of content in the site model
      siteModel.StorageRepresentationToSupply.Should().Be(StorageMutability.Immutable);
      siteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      return siteModel;
    }

    /// <summary>
    /// Constructs a simple TIN model containing a single triangle at (-25, -25), (25, -25), (0, 25)
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="elevation"></param>
    /// <returns></returns>
    public static Guid ConstructSingleFlatTriangleDesignAboutOrigin(ref ISiteModel siteModel, float elevation)
    {
      // Make a mutable TIN containing a single triangle and as below and register it to the site model
      VSS.TRex.Designs.TTM.TrimbleTINModel tin = new TrimbleTINModel();
      tin.Vertices.InitPointSearch(-100, -100, 100, 100, 3);
      tin.Triangles.AddTriangle(tin.Vertices.AddPoint(-25, -25, elevation),
        tin.Vertices.AddPoint(25, -25, elevation),
        tin.Vertices.AddPoint(0, 25, elevation));

      var tempFileName = Path.GetTempFileName() + ".ttm";
      tin.SaveToFile(tempFileName, true);

      return DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel
        (ref siteModel, Path.GetDirectoryName(tempFileName), Path.GetFileName(tempFileName), true);
    }

    /// <summary>
    /// Constructs a single triangle design surface as per ConstructSingleFlatTriangleDesignAboutOrigin but allows
    /// the triangle to be offset from the origin.
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="elevation"></param>
    /// <param name="asAtDate"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    /// <returns></returns>
    public static Guid ConstructSingleFlatTriangleSurveyedSurfaceOffsetFromOrigin(ref ISiteModel siteModel, float elevation, DateTime asAtDate, double offsetX, double offsetY)
    {
      // Make a mutable TIN containing a single triangle and as below and register it to the site model
      VSS.TRex.Designs.TTM.TrimbleTINModel tin = new TrimbleTINModel();
      tin.Vertices.InitPointSearch(-100 + offsetX, -100 + offsetY, 100 + offsetX, 100 + offsetY, 3);
      tin.Triangles.AddTriangle(tin.Vertices.AddPoint(-25 + offsetX, -25, elevation),
        tin.Vertices.AddPoint(25 + offsetX, -25 + offsetY, elevation),
        tin.Vertices.AddPoint(0 + offsetX, 25 + offsetY, elevation));

      var tempFileName = Path.GetTempFileName() + ".ttm";
      tin.SaveToFile(tempFileName, true);

      return DITAGFileAndSubGridRequestsWithIgniteFixture.AddSurveyedSurfaceToSiteModel
        (ref siteModel, Path.GetDirectoryName(tempFileName), Path.GetFileName(tempFileName), asAtDate, true);
    }

    /// <summary>
    /// Constructs a surveyed surface of the same TIN produced by ConstructSingleFlatTriangleDesignAboutOrigin and
    /// with the supplied AsAtData to the site model
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="elevation"></param>
    /// <param name="asAtDate"></param>
    /// <returns></returns>
    public static Guid ConstructSingleFlatTriangleSurveyedSurfaceAboutOrigin(ref ISiteModel siteModel, float elevation, DateTime asAtDate)
    {
      return ConstructSingleFlatTriangleSurveyedSurfaceOffsetFromOrigin(ref siteModel, elevation, asAtDate, 0, 0);
    }

    /// <summary>
    /// Constructs a flat design surface comprising two triangles covering the exents of a provided sitemodel.
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="elevation"></param>
    /// <returns></returns>
    public static Guid ConstructFlatTTMDesignEncompassingSiteModel(ref ISiteModel siteModel, float elevation)
    {
      // Make a mutable TIN containing two as below and register it to the site model
      var extent = siteModel.SiteModelExtent;
      var tin = new TrimbleTINModel();
      tin.Vertices.InitPointSearch(extent.MinX - 100, extent.MinY - 100, extent.MaxX + 100, extent.MaxX + 100, 6);

      tin.Triangles.AddTriangle(
        tin.Vertices.AddPoint(extent.MinX - 100, extent.MinY - 100, elevation),
        tin.Vertices.AddPoint(extent.MinX - 100, extent.MaxY + 100, elevation),
        tin.Vertices.AddPoint(extent.MaxX + 100, extent.MinY - 100, elevation));

      tin.Triangles.AddTriangle(
        tin.Vertices.AddPoint(extent.MinX - 100, extent.MaxY + 100, elevation),
        tin.Vertices.AddPoint(extent.MaxX + 100, extent.MaxY + 100, elevation),
        tin.Vertices.AddPoint(extent.MaxX + 100, extent.MinY - 100, elevation));

      var tempFileName = Path.GetTempFileName() + ".ttm";
      tin.SaveToFile(tempFileName, true);

      return DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(
        ref siteModel, Path.GetDirectoryName(tempFileName), Path.GetFileName(tempFileName), true);
    }

    public new void Dispose()
    {
      base.Dispose();
    }
  }
}
