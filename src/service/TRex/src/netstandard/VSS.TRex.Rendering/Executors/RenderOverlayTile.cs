using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using CoreX.Interfaces;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using VSS.Productivity3D.Models.Enums;
using VSS.Serilog.Extensions;
using VSS.TRex.Common;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.RequestStatistics;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Rendering.Displayers;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Executors
{
  /// <summary>
  /// Renders a tile of thematic imagery for a location in the project
  /// </summary>
  public class RenderOverlayTile
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RenderOverlayTile>();

    /// <summary>
    /// Details the error status of the bmp result returned by the renderer
    /// </summary>
    public RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private Guid RequestingTRexNodeID { get; }

    private readonly Guid DataModelID;
    private readonly DisplayMode Mode;

    private readonly bool CoordsAreGrid;

    private readonly XYZ BLPoint;
    private readonly XYZ TRPoint;

    private XYZ[] NEECoords;
    private XYZ[] LLHCoords;

    private readonly ushort NPixelsX;
    private readonly ushort NPixelsY;

    private double TileRotation;
    private double WorldTileWidth, WorldTileHeight;

    private readonly IFilterSet Filters;
    private readonly VolumeComputationType VolumeType;

    /// <summary>
    /// The identifier for the design held in the designs list of the project to be used to calculate cut/fill values
    /// together with the offset if it's a reference surface
    /// </summary>
    public DesignOffset CutFillDesign { get; set; }

    private readonly IPlanViewPalette ColorPalettes;
    private readonly Color RepresentColor;

    private readonly ILiftParameters LiftParams;

    /// <summary>
    /// Constructor for the renderer
    /// </summary>
    public RenderOverlayTile(Guid dataModelId,
      //AExternalDescriptor :TASNodeRequestDescriptor;
      DisplayMode mode,
      XYZ blPoint,
      XYZ trPoint,
      bool coordsAreGrid,
      ushort nPixelsX,
      ushort nPixelsY,
      IFilterSet filters,
      DesignOffset cutFillDesign,
      IPlanViewPalette aColorPalettes,
      Color representColor,
      Guid requestingTRexNodeId,
      ILiftParameters liftParams,
      VolumeComputationType volumeType
    )
    {
      DataModelID = dataModelId;
      // ExternalDescriptor = AExternalDescriptor
      Mode = mode;
      BLPoint = blPoint;
      TRPoint = trPoint;
      CoordsAreGrid = coordsAreGrid;
      NPixelsX = nPixelsX;
      NPixelsY = nPixelsY;
      Filters = filters;
      CutFillDesign = cutFillDesign;
      ColorPalettes = aColorPalettes;
      RepresentColor = representColor;
      RequestingTRexNodeID = requestingTRexNodeId;
      LiftParams = liftParams;
      VolumeType = volumeType;
    }

    private readonly BoundingWorldExtent3D RotatedTileBoundingExtents = BoundingWorldExtent3D.Inverted();

    /* TODO ColorPaletteClassType()
     private TICDisplayPaletteBaseClass ColorPaletteClassType()
        {
        case FMode of
          ...Height                  : Result  = TICDisplayPalette_Height;
          ...CCV                     : Result  = TICDisplayPalette_CCV;
          ...CCVPercent              : Result  = TICDisplayPalette_CCVPercent;
          ...Latency                 : Result  = TICDisplayPalette_RadioLatency;
          ...PassCount               : Result  = TICDisplayPalette_PassCount;
          ...PassCountSummary        : Result  = TICDisplayPalette_PassCountSummary;  // Palettes are fixed three color palettes - display will use direct transitions
          ...RMV                     : Result  = TICDisplayPalette_RMV;
          ...Frequency               : Result  = TICDisplayPalette_Frequency;
          ...Amplitude               : Result  = TICDisplayPalette_Amplitude;
          ...CutFill                 : Result  = TICDisplayPalette_CutFill;
          ...Moisture                : Result  = TICDisplayPalette_Moisture;
          ...TemperatureSummary      : Result  = TICDisplayPaletteBase; //TICDisplayPalette_Temperature;
          ...GPSMode                 : Result  = TICDisplayPaletteBase; //TICDisplayPalette_GPSMode;
          ...CCVSummary              : Result  = TICDisplayPaletteBase; //TICDisplayPalette_CCVSummary;
          ...CCVPercentSummary       : Result  = TICDisplayPalette_CCVPercent;
          ...CompactionCoverage      : Result  = TICDisplayPalette_CoverageOverlay;
          ...VolumeCoverage          : Result  = TICDisplayPalette_VolumeOverlay;
          ...MDP                     : Result  = TICDisplayPalette_MDP; // ajr15167
          ...MDPSummary              : Result  = TICDisplayPaletteBase;
          ...MDPPercent              : Result  = TICDisplayPalette_MDPPercent;
          ...MDPPercentSummary       : Result  = TICDisplayPalette_MDPPercent;
          ...MachineSpeed            : Result  = TICDisplayPalette_MachineSpeed;
          ...CCVPercentChange        : Result  = TICDisplayPalette_CCVPercent;
          ...TargetThicknessSummary  : Result  = TICDisplayPalette_VolumeOverlay;
          ...TargetSpeedSummary      : Result  = TICDisplayPalette_SpeedSummary;
          ...CCVChange               : Result  = TICDisplayPalette_CCVChange;
          ...CCA                     : Result  = TICDisplayPalette_CCA;
          ...CCASummary              : Result  = TICDisplayPalette_CCASummary;

        else
          SIGLogMessage.PublishNoODS(Self, Format('ColorPaletteClassType: Unknown display type: %d', [Ord(FMode)]), ...Assert);
          Result  = TICDisplayPaletteBase;
        end;
      end;
    */

    /* TODO: ComputeCCAPalette
      function ComputeCCAPalette :Boolean;
      var
        I, J, K               :Integer;
        ResponseVerb        :...VerbBase;
        ServerResult        :TICServerRequestResult;
        ResponseDataStream  :TStream;
        CCAMinimumPasses    :...CCAMinPassesValue;
        CCAColorScale       :...CCAColorScale;
        CCAPalette          :TColorPalettes;

      begin
        Result  = False;

        ResponseVerb  = nil;
        try
          if Length(FFilter1.Machines) > 0 then
            FMachineID  = FFilter1.Machines[0].ID // Must be set by caller
          else
            FMachineID  = -1; // will fail call
          if not Assigned(ASNodeImplInstance) or ASNodeImplInstance.ServiceStopped then
            begin
              SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Aborting request as service has been stopped', [Self.ClassName]), ...Warning);
              Exit;
            end;

          ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetMachineCCAMinimumPassesValue(FDataModelID, FMachineID, FFilter1.StartTime, FFilter1.EndTime, FFilter1.LayerID, ResponseVerb);
          if Assigned(ResponseVerb) then
            with ResponseVerb as ...Verb_SendResponse do
              begin
                ServerResult  = TICServerRequestResult(ResponseCode);
            ResponseDataStream  = ResponseData;
                if (ServerResult = ...NoError) and assigned(ResponseData) then
                  begin
                    CCAMinimumPasses  = ReadSmallIntFromStream(ResponseDataStream);

            Result  = CCAMinimumPasses > 0;

                    if not Result then
                      Exit;

            CCAColorScale  = ...CCAColorScaleManager.CreateCoverageScale(CCAMinimumPasses);
                    try
                      SetLength(CCAPalette.Transitions, CCAColorScale.TotalColors);

            J  = Low(CCAPalette.Transitions);
            k  = High(CCAPalette.Transitions);
                      for I  = J to K do
                        begin
                          CCAPalette.Transitions[I].Color  = CCAColorScale.ColorSegments[K - I].Color;
            CCAPalette.Transitions[I].Value   = I+1;
                        end;
                      CCAPalette.ConvertRGBToBGR; // gets done again but needed to match Anatoli palette test :)
                      WorkingColorPalette.PopulateFromPaletteColors(CCAPalette);
                      WorkingColorPalette.TransitionColors.ValuesCount  = Length(CCAPalette.Transitions);
                    finally
                      if Assigned(CCAColorScale) then
                        FreeAndNil(CCAColorScale);
            end;
                  end
                else
                  SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: GetMachineCCAMinimumPassesValue Failed for InternalSiteModelMachineIndex: %d. ReturnCode:%d', [Self.ClassName, FMachineID, Ord(ServerResult)]), ...Warning);
              end;
        finally
          if Assigned(ResponseVerb) then
            FreeAndNil(ResponseVerb);
            end;
      end;
    */

    /* TODO: CreateAndInitialiseWorkingColorPalette
     function CreateAndInitialiseWorkingColorPalette :Boolean;
     begin
       Result  = True;

       // Create a scaled palette to use when rendering the data
       try
         if ColorPaletteClassType<> Nil then
          begin

             WorkingColorPalette  = ColorPaletteClassType.Create;
             WorkingColorPalette.SmoothPalette  = FMode = ...CutFill;

             // CCASummary is done per machine id
             if FMode in [...CCA, ...CCASummary]
           then
               Result  = ComputeCCAPalette
             else
               begin
                 if Length(FColorPalettes.Transitions) = 0 then
                   WorkingColorPalette.SetToDefaults
                 else
                   WorkingColorPalette.PopulateFromPaletteColors(FColorPalettes);
           end;

             if Result then
               WorkingColorPalette.ComputePalette;
           end
         else
           WorkingColorPalette  = Nil;

       Except
         On e:Exception do
           SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Error: %s ', [Self.ClassName, e.Message]), ...Exception);
       end;
     end;
   */

    /// <summary>
    /// Renders all sub grids in a representational style that indicates where there is data, but nothing else. This is used for large scale displays
    /// (zoomed out a lot) where meaningful detail cannot be drawn on the tile
    /// </summary>
    private SKBitmap RenderTileAsRepresentationalDueToScale(ISubGridTreeBitMask overallExistenceMap)
    {
      using (var RepresentationalDisplay = PVMDisplayerFactory.GetDisplayer(Mode /*, FICOptions*/))
      {
        using (var mapView = new MapSurface { SquareAspect = false })
        {
          mapView.SetRotation(TileRotation);

          RepresentationalDisplay.MapView = mapView;

          RepresentationalDisplay.MapView.SetBounds(NPixelsX, NPixelsY);
          RepresentationalDisplay.MapView.SetWorldBounds(NEECoords[0].X, NEECoords[0].Y,
            NEECoords[0].X + WorldTileWidth, NEECoords[0].Y + WorldTileHeight, 0);

          // Iterate over all the bits in the sub grids drawing a rectangle for each one on the tile being rendered
          if (overallExistenceMap.ScanSubGrids(RotatedTileBoundingExtents,
            leaf =>
            {
              leaf.CalculateWorldOrigin(out var WorldOriginX, out var WorldOriginY);

              (leaf as SubGridTreeLeafBitmapSubGrid)?.Bits.ForEachSetBit((x, y) =>
              {
                RepresentationalDisplay.MapView.DrawRect(WorldOriginX + (x * overallExistenceMap.CellSize),
                  WorldOriginY + (y * overallExistenceMap.CellSize),
                  overallExistenceMap.CellSize, overallExistenceMap.CellSize, true, RepresentColor);
              });

              return true;
            }))
          {
            // Remove the canvas from the map view to prevent it's disposal (it's being returned to the caller)
            var canvas = RepresentationalDisplay.MapView.BitmapCanvas;
            RepresentationalDisplay.MapView.BitmapCanvas = null;
            return canvas;
          }
        }
      }

      return null; // It did not work out...
    }

    /// <summary>
    /// Executor that implements requesting and rendering sub grid information to create the rendered tile
    /// </summary>
    public async Task<SKBitmap> ExecuteAsync()
    {
      // WorkingColorPalette  : TICDisplayPaletteBase;

      _log.LogInformation($"Performing Execute for DataModel:{DataModelID}, Mode={Mode}");

      ApplicationServiceRequestStatistics.Instance.NumMapTileRequests.Increment();

      /*
     if Assigned(ASNodeImplInstance.RequestCancellations) and
        ASNodeImplInstance.RequestCancellations.IsRequestCancelled(FExternalDescriptor) then
       begin
         if ...SvcLocations.Debug_LogDebugRequestCancellationToFile then
           SIGLogMessage.PublishNoODS(Self, 'Request cancelled: ' + FExternalDescriptor.ToString, ...Debug);

         ResultStatus  = ...RequestHasBeenCancelled;
         InterlockedIncrement64(ASNodeRequestStats.NumMapTileRequestsCancelled);
       Exit;
       end;

      // The governor is intended to restrict the numbers of heavy weight processes
      // such as pipelines that interact with the PC layer to request sub grids
      ScheduledWithGovernor  = ASNodeImplInstance.Governor.Schedule(FExternalDescriptor, Self, gqWMS, ResultStatus);
      if not ScheduledWithGovernor then
        Exit;
      */

      var RequestDescriptor = Guid.NewGuid();

      if (_log.IsDebugEnabled())
      {
        if (CoordsAreGrid)
        {
          _log.LogDebug($"RenderPlanViewTiles Execute: Performing render for request={RequestDescriptor} Args: Project={DataModelID}, Mode={Mode}, CutFillDesign=''{CutFillDesign}'' " +
                       $"Bound[BL/TR:X/Y]=({BLPoint.X} {BLPoint.Y}, {TRPoint.X} {TRPoint.Y}), Width={NPixelsX}, Height={NPixelsY}");
        }
        else
        {
          _log.LogDebug($"RenderPlanViewTiles Execute: Performing render for request={RequestDescriptor} Args: Project={DataModelID}, Mode={Mode}, CutFillDesign=''{CutFillDesign}'' " +
                       $"Bound[BL/TR:Lon/Lat]=({BLPoint.X} {BLPoint.Y}, {TRPoint.X} {TRPoint.Y}), Width={NPixelsX}, Height={NPixelsY}");
        }

        // Include the details of the filters with the logged tile parameters
        if (Filters != null)
        {
          for (var i = 0; i < Filters.Filters.Length; i++)
          {
            _log.LogDebug($"Filter({i}): {Filters.Filters[i]}");
          }
        }
      }

      // Determine the grid (NEE) coordinates of the bottom/left, top/right WGS-84 positions
      // given the project's coordinate system. If there is no coordinate system then exit.

      var SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);
      if (SiteModel == null)
      {
        _log.LogWarning($"Failed to locate site model {DataModelID}");
        return null;
      }

      _log.LogInformation($"Got Site model {DataModelID}, production data extents are {SiteModel.SiteModelExtent}");

      LLHCoords = new[]
      {
        new XYZ(BLPoint.X, BLPoint.Y, 0),
        new XYZ(TRPoint.X, TRPoint.Y, 0),
        new XYZ(BLPoint.X, TRPoint.Y, 0),
        new XYZ(TRPoint.X, BLPoint.Y, 0)
      };
      _log.LogInformation($"LLHCoords for tile request {string.Concat(LLHCoords)}, CoordsAreGrid {CoordsAreGrid}");

      if (CoordsAreGrid)
        NEECoords = LLHCoords;
      else
      {
        NEECoords = DIContext
          .Obtain<IConvertCoordinates>()
          .LLHToNEE(SiteModel.CSIB(), LLHCoords.ToCoreX_XYZ(), CoreX.Types.InputAs.Radians)
          .ToTRex_XYZ();
      }
      _log.LogInformation($"After conversion NEECoords are {string.Concat(NEECoords)}");

      WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
      WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

      var dx = NEECoords[2].X - NEECoords[0].X;
      var dy = NEECoords[2].Y - NEECoords[0].Y;

      // Calculate the tile rotation as the mathematical angle turned from 0 (due east) to the vector defined by dy/dx
      TileRotation = Math.Atan2(dy, dx);

      // Convert TileRotation to represent the angular deviation rather than a bearing
      TileRotation = (Math.PI / 2) - TileRotation;

      RotatedTileBoundingExtents.SetInverted();
      NEECoords.ForEach(xyz => RotatedTileBoundingExtents.Include(xyz.X, xyz.Y));

      _log.LogInformation($"Tile render executing across tile: [Rotation:{TileRotation}, {MathUtilities.RadiansToDegrees(TileRotation)} degrees] " +
        $" [BL:{NEECoords[0].X}, {NEECoords[0].Y}, TL:{NEECoords[2].X},{NEECoords[2].Y}, " +
        $"TR:{NEECoords[1].X}, {NEECoords[1].Y}, BR:{NEECoords[3].X}, {NEECoords[3].Y}] " +
        $"World Width, Height: {WorldTileWidth}, {WorldTileHeight}, Rotated bounding extents: {RotatedTileBoundingExtents}");

      // Construct the renderer, configure it, and set it on its way
      //  WorkingColorPalette = Nil;

      using (var Renderer = new PlanViewTileRenderer())
      {
        try
        {
          // Intersect the site model extents with the extents requested by the caller
          var adjustedSiteModelExtents = SiteModel.GetAdjustedDataModelSpatialExtents(null);

          _log.LogInformation($"Calculating intersection of bounding box and site model {DataModelID}:{adjustedSiteModelExtents}");

          var dataSelectionExtent = new BoundingWorldExtent3D(RotatedTileBoundingExtents);
          dataSelectionExtent.Intersect(adjustedSiteModelExtents);
          if (!dataSelectionExtent.IsValidPlanExtent)
          {
            ResultStatus = RequestErrorStatus.InvalidCoordinateRange;
            _log.LogInformation($"Site model extents {adjustedSiteModelExtents}, do not intersect RotatedTileBoundingExtents {RotatedTileBoundingExtents}");

            using var mapView = new MapSurface();

            mapView.SetBounds(NPixelsX, NPixelsY);

            var canvas = mapView.BitmapCanvas;
            mapView.BitmapCanvas = null;

            return canvas;
          }

          // Compute the override cell boundary to be used when processing cells in the sub grids
          // selected as a part of this pipeline
          // Increase cell boundary by one cell to allow for cells on the boundary that cross the boundary

          SubGridTree.CalculateIndexOfCellContainingPosition(dataSelectionExtent.MinX,
            dataSelectionExtent.MinY, SiteModel.CellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
            out var CellExtents_MinX, out var CellExtents_MinY);
          SubGridTree.CalculateIndexOfCellContainingPosition(dataSelectionExtent.MaxX,
            dataSelectionExtent.MaxY, SiteModel.CellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
            out var CellExtents_MaxX, out var CellExtents_MaxY);

          var CellExtents = new BoundingIntegerExtent2D(CellExtents_MinX, CellExtents_MinY, CellExtents_MaxX, CellExtents_MaxY);
          CellExtents.Expand(1);

          var filterSet = FilterUtilities.ConstructFilters(Filters, VolumeType);
          // Construct PipelineProcessor
          using var processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild<SubGridsRequestArgument>(
            RequestDescriptor,
            DataModelID,
            GridDataFromModeConverter.Convert(Mode),
            new SubGridsPipelinedResponseBase(),
            filterSet,
            CutFillDesign,
            DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.PVMRendering),
            DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
            DIContext.Obtain<IRequestAnalyser>(),
            Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode) && Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
            requestRequiresAccessToDesignFileExistenceMap: Utilities.RequestRequiresAccessToDesignFileExistenceMap(Mode, CutFillDesign),
            CellExtents,
            LiftParams
          );
          if (filterSet.Filters.Length == 3)
          {
            var pipeline = processor.Pipeline as SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>;
            pipeline.SubGridsRequestComputeStyle = SubGridsRequestComputeStyle.SimpleVolumeThreeWayCoalescing;
          }

          // Set the PVM rendering rexTask parameters for progressive processing
          processor.Task.TRexNodeID = RequestingTRexNodeID;
          ((IPVMRenderingTask)processor.Task).TileRenderer = Renderer;

          // Set the spatial extents of the tile boundary rotated into the north reference frame of the cell coordinate system to act as
          // a final restriction of the spatial extent used to govern data requests
          processor.OverrideSpatialExtents.Assign(RotatedTileBoundingExtents);

          // Prepare the processor
          if (!await processor.BuildAsync())
          {
            _log.LogError($"Failed to build pipeline processor for request to model {SiteModel.ID}");
            ResultStatus = RequestErrorStatus.FailedToConfigureInternalPipeline;
            return null;
          }

          // Test to see if the tile can be satisfied with a representational render indicating where
          // data is but not what it is (this is useful when the zoom level is far enough away that we
          // cannot meaningfully render the data). If the size of s sub grid is smaller than
          // the size of a pixel in the requested tile then do this. Just check the X dimension
          // as the data display is isotropic.
          if (Utilities.SubGridShouldBeRenderedAsRepresentationalDueToScale(WorldTileWidth, WorldTileHeight, NPixelsX, NPixelsY, processor.OverallExistenceMap.CellSize))
            return RenderTileAsRepresentationalDueToScale(processor.OverallExistenceMap); // There is no need to do anything else

          /* TODO - Create a scaled palette to use when rendering the data
            // Create a scaled palette to use when rendering the data
            if not CreateAndInitialiseWorkingColorPalette then
              begin
              SIGLogMessage.PublishNoODS(Self, Format('Failed to create and initialise working color palette for data: %s in datamodel %d', [TypInfo.GetEnumName(TypeInfo(TICDisplayMode), Ord(FMode)), FDataModelID]), ...Warning);
              Exit;
              end;
            */

          // Renderer.WorkingPalette = WorkingColorPalette;

          Renderer.IsWhollyInTermsOfGridProjection = true; // Ensure the renderer knows we are using grid projection coordinates

          Renderer.SetBounds(RotatedTileBoundingExtents.CenterX - WorldTileWidth / 2,
                             RotatedTileBoundingExtents.CenterY - WorldTileHeight / 2,
                             WorldTileWidth, WorldTileHeight,
                             NPixelsX, NPixelsY);
          Renderer.TileRotation = TileRotation;

          var performRenderStopWatch = Stopwatch.StartNew();
          ResultStatus = Renderer.PerformRender(Mode, processor, ColorPalettes, Filters, LiftParams);
          _log.LogInformation($"Renderer.PerformRender completed in {performRenderStopWatch.Elapsed}");

          if (processor.Response.ResultStatus == RequestErrorStatus.OK)
          {
            var canvas = Renderer.Displayer.MapView.BitmapCanvas;
            Renderer.Displayer.MapView.BitmapCanvas = null;
            return canvas;
          }
        }
        catch (Exception e)
        {
          _log.LogError(e, "Exception occurred");
          ResultStatus = RequestErrorStatus.Exception;
        }
      }

      return null;
    }
  }
}
