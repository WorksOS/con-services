using System;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Displayers;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.Common.RequestStatistics;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using System.Drawing;
using System.Threading.Tasks;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Executors
{
  /// <summary>
  /// Renders a tile of thematic imagery for a location in the project
  /// </summary>
  public class RenderOverlayTile
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<RenderOverlayTile>();

    /// <summary>
    /// Details the error status of the bmp result returned by the renderer
    /// </summary>
    public RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; }

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

    /// <summary>
    /// The identifier for the design held in the designs list of the project to be used to calculate cut/fill values
    /// together with the offset if it's a reference surface
    /// </summary>
    public DesignOffset CutFillDesign { get; set; }

    // ComputeICVolumesType ReferenceVolumeType = ComputeICVolumesType.None;
    private readonly IPlanViewPalette ColorPalettes;
    // ICOptions ICOptions = new ICOptions();
    private readonly Color RepresentColor;

    private readonly ILiftParameters LiftParams;

    /// <summary>
    /// Constructor for the renderer
    /// </summary>
    public RenderOverlayTile(Guid ADataModelID,
      //AExternalDescriptor :TASNodeRequestDescriptor;
      DisplayMode AMode,
      XYZ ABLPoint,
      XYZ ATRPoint,
      bool ACoordsAreGrid,
      ushort ANPixelsX,
      ushort ANPixelsY,
      IFilterSet filters,
      DesignOffset ACutFillDesign, 
      //AReferenceVolumeType : TComputeICVolumesType;
      IPlanViewPalette aColorPalettes,
      //AICOptions: ...ICOptions;
      Color ARepresentColor,
      string requestingTRexNodeId,
      ILiftParameters liftParams
    )
    {
      DataModelID = ADataModelID;
      // ExternalDescriptor = AExternalDescriptor
      Mode = AMode;
      BLPoint = ABLPoint;
      TRPoint = ATRPoint;
      CoordsAreGrid = ACoordsAreGrid;
      NPixelsX = ANPixelsX;
      NPixelsY = ANPixelsY;
      Filters = filters;
      CutFillDesign = ACutFillDesign; 
      //ReferenceVolumeType = AReferenceVolumeType;
      ColorPalettes = aColorPalettes;
      //ICOptions = AICOptions;
      RepresentColor = ARepresentColor;
      RequestingTRexNodeID = requestingTRexNodeId;
      LiftParams = liftParams;
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
    /// <returns></returns>
    private IBitmap RenderTileAsRepresentationalDueToScale(ISubGridTreeBitMask overallExistenceMap)
    {
      var RepresentationalDisplay = PVMDisplayerFactory.GetDisplayer(Mode /*, FICOptions*/);

      RepresentationalDisplay.MapView = new MapSurface
      {
        SquareAspect = false,
        Rotation = -TileRotation + Math.PI / 2
      };

      RepresentationalDisplay.MapView.SetBounds(NPixelsX, NPixelsY);
      RepresentationalDisplay.MapView.SetWorldBounds(NEECoords[0].X, NEECoords[0].Y,
        NEECoords[0].X + WorldTileWidth, NEECoords[0].Y + WorldTileHeight, 0);

      // Iterate over all the bits in the sub grids drawing a rectangle for each one on the tile being rendered
      if (overallExistenceMap.ScanSubGrids(RotatedTileBoundingExtents,
        leaf =>
        {
          leaf.CalculateWorldOrigin(out double WorldOriginX, out double WorldOriginY);

          (leaf as SubGridTreeLeafBitmapSubGrid)?.Bits.ForEachSetBit((x, y) =>
          {
            RepresentationalDisplay.MapView.DrawRect(WorldOriginX + (x * overallExistenceMap.CellSize),
              WorldOriginY + (y * overallExistenceMap.CellSize),
              overallExistenceMap.CellSize, overallExistenceMap.CellSize, true, RepresentColor);
          });

          return true;
        }))
      {
        return RepresentationalDisplay.MapView.BitmapCanvas;
      }

      return null; // It did not work out...
    }

    /// <summary>
    /// Executor that implements requesting and rendering sub grid information to create the rendered tile
    /// </summary>
    public async Task<IBitmap> ExecuteAsync()
    {
      // WorkingColorPalette  : TICDisplayPaletteBase;

      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}, Mode={Mode}");

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

      /* TODO Re-add when logging available
    if VLPDSvcLocations.Debug_EmitTileRenderRequestParametersToLog then
      begin
        if FCoordsAreGrid then
          begin
            if Assigned(FFilter2) then
              SIGLogMessage.PublishNoODS(Self,
                                         Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                'Args: Project=%d, Mode=%d, Filter1=%d, Filter2=%d, Design=''%s'', VolType=%d, Bound[BL/TR:X/Y]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                [RequestDescriptor,
                                                 FDataModelID,
                                                 Integer(FMode),
                                                 FFilter1.FilterID, FFilter2.FilterID,
                                                 FDesignDescriptor.ToString,
                                                 Ord(FReferenceVolumeType),
                                                 FBLPoint.Lon, FBLPoint.Lat, FTRPoint.Lon, FTRPoint.Lat,
                                                 FNPixelsX, FNPixelsY]),
                                         ...Message)
            else
              SIGLogMessage.PublishNoODS(Self,
                                         Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                'Args: Project=%d, Mode=%d, Filter1=%d Design=''%s'', VolType=%d, Bound[BL/TR:X/Y]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                [RequestDescriptor,
                                                 FDataModelID,
                                                 Integer(FMode),
                                                 FFilter1.FilterID,
                                                 FDesignDescriptor.ToString,
                                                 Ord(FReferenceVolumeType),
                                                 FBLPoint.Lon, FBLPoint.Lat, FTRPoint.Lon, FTRPoint.Lat,
                                                 FNPixelsX, FNPixelsY]),
                                         ...Message);
          end
        else
          begin
            if Assigned(FFilter2) then
              SIGLogMessage.PublishNoODS(Self,
                                         Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                'Args: Project=%d, Mode=%d, Filter1=%d, Filter2=%d, Design=''%s'', VolType=%d, Bound[BL/TR:Lon/Lat]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                [RequestDescriptor,
                                                 FDataModelID,
                                                 Integer(FMode),
                                                 FFilter1.FilterID, FFilter2.FilterID,
                                                 FDesignDescriptor.ToString,
                                                 Ord(FReferenceVolumeType),
                                                 FBLPoint.Lon * (180/PI), FBLPoint.Lat* (180/PI), FTRPoint.Lon* (180/PI), FTRPoint.Lat* (180/PI),
                                                 FNPixelsX, FNPixelsY]),
                                         ...Message)
            else
              SIGLogMessage.PublishNoODS(Self,
                                         Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                'Args: Project=%d, Mode=%d, Filter1=%d, Design=''%s'', VolType=%d, Bound[BL/TR:Lon/Lat]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                [RequestDescriptor,
                                                 FDataModelID,
                                                 Integer(FMode),
                                                 FFilter1.FilterID,
                                                 FDesignDescriptor.ToString,
                                                 Ord(FReferenceVolumeType),
                                                 FBLPoint.Lon * (180/PI), FBLPoint.Lat* (180/PI), FTRPoint.Lon* (180/PI), FTRPoint.Lat* (180/PI),
                                                 FNPixelsX, FNPixelsY]),
                                         ...Message);
          end;

        // Include the details of the filter with the logged tile parameters
        SIGLogMessage.PublishNoODS(Self, Format('Filter1: %s', [IfThen(FFilter1.IsNull, 'Null', FFilter1.ToString)]), ...Message);
        if Assigned(FFilter2) then
          SIGLogMessage.PublishNoODS(Self, Format('Filter2: %s', [IfThen(FFilter2.IsNull, 'Null', FFilter2.ToString)]), ...Message);
      end;
      */

      // Determine the grid (NEE) coordinates of the bottom/left, top/right WGS-84 positions
      // given the project's coordinate system. If there is no coordinate system then exit.

      var SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);
      Log.LogInformation($"Got Site model {DataModelID}, extents are {SiteModel.SiteModelExtent}");

      LLHCoords = new[]
      {
        new XYZ(BLPoint.X, BLPoint.Y),
        new XYZ(TRPoint.X, TRPoint.Y),
        new XYZ(BLPoint.X, TRPoint.Y),
        new XYZ(TRPoint.X, BLPoint.Y)
      };
      Log.LogInformation($"LLHCoords for tile request {string.Concat(LLHCoords)}, CoordsAreGrid {CoordsAreGrid}");

      if (CoordsAreGrid)
        NEECoords = LLHCoords;
      else
      {
        var conversionResult = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(SiteModel.CSIB(), LLHCoords);

        if (conversionResult.ErrorCode != RequestErrorStatus.OK)
        {
          Log.LogInformation("Tile render failure, could not convert bounding area from WGS to grid coordinates");
          ResultStatus = RequestErrorStatus.FailedToConvertClientWGSCoords;

          return null;
        }

        NEECoords = conversionResult.NEECoordinates;
      }
      Log.LogInformation($"After conversion NEECoords are {string.Concat(NEECoords)}");

      WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
      WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

      double dx = NEECoords[2].X - NEECoords[0].X;
      double dy = NEECoords[2].Y - NEECoords[0].Y;
      TileRotation = Math.PI / 2 - Math.Atan2(dy, dx);

      RotatedTileBoundingExtents.SetInverted();
      foreach (var xyz in NEECoords)
        RotatedTileBoundingExtents.Include(xyz.X, xyz.Y);

      Log.LogInformation($"Tile render executing across tile: [Rotation:{TileRotation}] " +
        $" [BL:{NEECoords[0].X}, {NEECoords[0].Y}, TL:{NEECoords[2].X},{NEECoords[2].Y}, " +
        $"TR:{NEECoords[1].X}, {NEECoords[1].Y}, BR:{NEECoords[3].X}, {NEECoords[3].Y}] " +
        $"World Width, Height: {WorldTileWidth}, {WorldTileHeight}" );
      
      // Construct the renderer, configure it, and set it on its way
      //  WorkingColorPalette = Nil;

      var Renderer = new PlanViewTileRenderer();
      try
      {
        // Intersect the site model extents with the extents requested by the caller
        Log.LogInformation($"Calculating intersection of bounding box and site model {DataModelID}:{SiteModel.SiteModelExtent}");
        RotatedTileBoundingExtents.Intersect(SiteModel.SiteModelExtent);
        if (!RotatedTileBoundingExtents.IsValidPlanExtent)
        {
          ResultStatus = RequestErrorStatus.InvalidCoordinateRange;
          Log.LogInformation($"Site model extents {SiteModel.SiteModelExtent}, do not intersect RotatedTileBoundingExtents {RotatedTileBoundingExtents}");

          var transparentDisplay = PVMDisplayerFactory.GetDisplayer(Mode);
          transparentDisplay.MapView = new MapSurface();
          transparentDisplay.MapView.SetBounds(NPixelsX, NPixelsY);
          return transparentDisplay.MapView.BitmapCanvas;
        }

        // Compute the override cell boundary to be used when processing cells in the sub grids
        // selected as a part of this pipeline
        // Increase cell boundary by one cell to allow for cells on the boundary that cross the boundary

        SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MinX,
          RotatedTileBoundingExtents.MinY, SiteModel.CellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
          out var CellExtents_MinX, out var CellExtents_MinY);
        SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MaxX,
          RotatedTileBoundingExtents.MaxY, SiteModel.CellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
          out var CellExtents_MaxX, out var CellExtents_MaxY);

        var CellExtents = new BoundingIntegerExtent2D(CellExtents_MinX, CellExtents_MinY, CellExtents_MaxX, CellExtents_MaxY);
        CellExtents.Expand(1);

        // Construct PipelineProcessor
        var processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(
          RequestDescriptor,
          DataModelID,
          GridDataFromModeConverter.Convert(Mode),
          new SubGridsPipelinedResponseBase(),
          Filters,
          CutFillDesign,
          DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.PVMRendering),
          DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          DIContext.Obtain<IRequestAnalyser>(),
          Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode) &&
                                             Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
          Utilities.RequestRequiresAccessToDesignFileExistenceMap(Mode /*ReferenceVolumeType*/),
          CellExtents,
          LiftParams
        );

        // Set the PVM rendering rexTask parameters for progressive processing
        processor.Task.RequestDescriptor = RequestDescriptor;
        processor.Task.TRexNodeID = RequestingTRexNodeID;
        processor.Task.GridDataType = GridDataFromModeConverter.Convert(Mode);
        ((IPVMRenderingTask)processor.Task).TileRenderer = Renderer;

        // Set the spatial extents of the tile boundary rotated into the north reference frame of the cell coordinate system to act as
        // a final restriction of the spatial extent used to govern data requests
        processor.OverrideSpatialExtents = RotatedTileBoundingExtents;

        // Prepare the processor
        if (!await processor.BuildAsync())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {SiteModel.ID}");
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
        // Renderer.ReferenceVolumeType = FReferenceVolumeType;

        Renderer.IsWhollyInTermsOfGridProjection = true; // Ensure the renderer knows we are using grid projection coordinates
        Renderer.SetBounds(NEECoords[0].X, NEECoords[0].Y, WorldTileWidth, WorldTileHeight, NPixelsX, NPixelsY);
        Renderer.TileRotation = TileRotation;
        Renderer.WorldTileWidth = WorldTileWidth;
        Renderer.WorldTileHeight = WorldTileHeight;

        ResultStatus = Renderer.PerformRender(Mode, processor, ColorPalettes, Filters, LiftParams);

        if (processor.Response.ResultStatus == RequestErrorStatus.OK)
          return Renderer.Displayer.MapView.BitmapCanvas;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred");
        ResultStatus = RequestErrorStatus.Exception;
      }

      return null;
    }
  }
}
