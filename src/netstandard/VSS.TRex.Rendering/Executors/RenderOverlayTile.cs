using Microsoft.Extensions.Logging;
using System;
using Draw = System.Drawing;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Displayers;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.RequestStatistics;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities;

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
    private string RequestingTRexNodeID { get; set; }

    private Guid DataModelID;
    private DisplayMode Mode;

    private bool CoordsAreGrid;

    private XYZ BLPoint; // : TWGS84Point;
    private XYZ TRPoint; // : TWGS84Point;

    private XYZ[] NEECoords;
    private XYZ[] LLHCoords;

    private ushort NPixelsX;
    private ushort NPixelsY;

    private double TileRotation;
    private double WorldTileWidth, WorldTileHeight;

    private ICombinedFilter Filter1;
    private ICombinedFilter Filter2;

    /// <summary>
    /// The identifier for the design held in the designs list ofr the project to be used to calculate cut/fill values
    /// </summary>
    public Guid CutFillDesignID { get; set; }

    // ComputeICVolumesType ReferenceVolumeType = ComputeICVolumesType.None;
    // FColourPalettes: TColourPalettes;
    // ICOptions ICOptions = new ICOptions();
    private Draw.Color RepresentColor;

    /// <summary>
    /// Constructor for the renderer
    /// </summary>
    /// <param name="ADataModelID"></param>
    /// <param name="AMode"></param>
    /// <param name="ABLPoint"></param>
    /// <param name="ATRPoint"></param>
    /// <param name="ACoordsAreGrid"></param>
    /// <param name="ANPixelsX"></param>
    /// <param name="ANPixelsY"></param>
    /// <param name="AFilter1"></param>
    /// <param name="AFilter2"></param>
    /// <param name="ACutFillDesignID"></param>
    /// <param name="ARepresentColor"></param>
    /// <param name="requestingTRexNodeId"></param>
    public RenderOverlayTile(Guid ADataModelID,
      //AExternalDescriptor :TASNodeRequestDescriptor;
      DisplayMode AMode,
      XYZ ABLPoint, // : TWGS84Point;
      XYZ ATRPoint, // : TWGS84Point;
      bool ACoordsAreGrid,
      ushort ANPixelsX,
      ushort ANPixelsY,
      ICombinedFilter AFilter1,
      ICombinedFilter AFilter2,
      Guid ACutFillDesignID, //DesignDescriptor ACutFillDesign,
                             //AReferenceVolumeType : TComputeICVolumesType;
                             //AColourPalettes: TColourPalettes;
                             //AICOptions: TSVOICOptions;
      Draw.Color ARepresentColor,
      string requestingTRexNodeId
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
      Filter1 = AFilter1;
      Filter2 = AFilter2;
      CutFillDesignID = ACutFillDesignID; // CutFillDesign = ACutFillDesign;
      //ReferenceVolumeType = AReferenceVolumeType;
      //ColourPalettes = AColourPalettes;
      //ICOptions = AICOptions;
      RepresentColor = ARepresentColor;
      RequestingTRexNodeID = requestingTRexNodeId;
    }

//        private SubGridTreeSubGridExistenceBitMask OverallExistenceMap;

    private BoundingWorldExtent3D RotatedTileBoundingExtents = BoundingWorldExtent3D.Inverted();

    /* TODO ColourPaletteClassType()
     private TICDisplayPaletteBaseClass ColourPaletteClassType()
        {
        case FMode of
          icdmHeight                  : Result  = TICDisplayPalette_Height;
          icdmCCV                     : Result  = TICDisplayPalette_CCV;
          icdmCCVPercent              : Result  = TICDisplayPalette_CCVPercent;
          icdmLatency                 : Result  = TICDisplayPalette_RadioLatency;
          icdmPassCount               : Result  = TICDisplayPalette_PassCount;
          icdmPassCountSummary        : Result  = TICDisplayPalette_PassCountSummary;  // Palettes are fixed three colour palettes - displayer will use direct transitions
          icdmRMV                     : Result  = TICDisplayPalette_RMV;
          icdmFrequency               : Result  = TICDisplayPalette_Frequency;
          icdmAmplitude               : Result  = TICDisplayPalette_Amplitude;
          icdmCutFill                 : Result  = TICDisplayPalette_CutFill;
          icdmMoisture                : Result  = TICDisplayPalette_Moisture;
          icdmTemperatureSummary      : Result  = TICDisplayPaletteBase; //TICDisplayPalette_Temperature;
          icdmGPSMode                 : Result  = TICDisplayPaletteBase; //TICDisplayPalette_GPSMode;
          icdmCCVSummary              : Result  = TICDisplayPaletteBase; //TICDisplayPalette_CCVSummary;
          icdmCCVPercentSummary       : Result  = TICDisplayPalette_CCVPercent;
          icdmCompactionCoverage      : Result  = TICDisplayPalette_CoverageOverlay;
          icdmVolumeCoverage          : Result  = TICDisplayPalette_VolumeOverlay;
          icdmMDP                     : Result  = TICDisplayPalette_MDP; // ajr15167
          icdmMDPSummary              : Result  = TICDisplayPaletteBase;
          icdmMDPPercent              : Result  = TICDisplayPalette_MDPPercent;
          icdmMDPPercentSummary       : Result  = TICDisplayPalette_MDPPercent;
          icdmMachineSpeed            : Result  = TICDisplayPalette_MachineSpeed;
          icdmCCVPercentChange        : Result  = TICDisplayPalette_CCVPercent;
          icdmTargetThicknessSummary  : Result  = TICDisplayPalette_VolumeOverlay;
          icdmTargetSpeedSummary      : Result  = TICDisplayPalette_SpeedSummary;
          icdmCCVChange               : Result  = TICDisplayPalette_CCVChange;
          icdmCCA                     : Result  = TICDisplayPalette_CCA;
          icdmCCASummary              : Result  = TICDisplayPalette_CCASummary;

        else
          SIGLogMessage.PublishNoODS(Self, Format('ColourPaletteClassType: Unknown display type: %d', [Ord(FMode)]), slmcAssert);
          Result  = TICDisplayPaletteBase;
        end;
      end;
    */

    /* TODO: ComputeCCAPalette
      function ComputeCCAPalette :Boolean;
      var
        I, J, K               :Integer;
        ResponseVerb        :TRPCVerbBase;
        ServerResult        :TICServerRequestResult;
        ResponseDataStream  :TStream;
        CCAMinimumPasses    :TICCCAMinPassesValue;
        CCAColorScale       :TCCAColorScale;
        CCAPalette          :TColourPalettes;

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
              SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Aborting request as service has been stopped', [Self.ClassName]), slmcWarning);
              Exit;
            end;

          ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetMachineCCAMinimumPassesValue(FDataModelID, FMachineID, FFilter1.StartTime, FFilter1.EndTime, FFilter1.LayerID, ResponseVerb);
          if Assigned(ResponseVerb) then
            with ResponseVerb as TVLPDRPCVerb_SendResponse do
              begin
                ServerResult  = TICServerRequestResult(ResponseCode);
            ResponseDataStream  = ResponseData;
                if (ServerResult = icsrrNoError) and assigned(ResponseData) then
                  begin
                    CCAMinimumPasses  = ReadSmallIntFromStream(ResponseDataStream);

            Result  = CCAMinimumPasses > 0;

                    if not Result then
                      Exit;

            CCAColorScale  = TCCAColorScaleManager.CreateCoverageScale(CCAMinimumPasses);
                    try
                      SetLength(CCAPalette.Transitions, CCAColorScale.TotalColors);

            J  = Low(CCAPalette.Transitions);
            k  = High(CCAPalette.Transitions);
                      for I  = J to K do
                        begin
                          CCAPalette.Transitions[I].Colour  = CCAColorScale.ColorSegments[K - I].Color;
            CCAPalette.Transitions[I].Value   = I+1;
                        end;
                      CCAPalette.ConvertRGBToBGR; // gets done again but needed to match Anatoli palette test :)
                      WorkingColourPalette.PopulateFromPaletteColours(CCAPalette);
                      WorkingColourPalette.TransitionColours.ValuesCount  = Length(CCAPalette.Transitions);
                    finally
                      if Assigned(CCAColorScale) then
                        FreeAndNil(CCAColorScale);
            end;
                  end
                else
                  SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: GetMachineCCAMinimumPassesValue Failed for MachineID: %d. ReturnCode:%d', [Self.ClassName, FMachineID, Ord(ServerResult)]), slmcWarning);
              end;
        finally
          if Assigned(ResponseVerb) then
            FreeAndNil(ResponseVerb);
            end;
      end;
    */

    /* TODO: CreateAndInitialiseWorkingColourPalette
     function CreateAndInitialiseWorkingColourPalette :Boolean;
     begin
       Result  = True;

       // Create a scaled palette to use when rendering the data
       try
         if ColourPaletteClassType<> Nil then
          begin

             WorkingColourPalette  = ColourPaletteClassType.Create;
             WorkingColourPalette.SmoothPalette  = FMode = icdmCutFill;

             // CCASummary is done per machineid
             if FMode in [icdmCCA, icdmCCASummary]
           then
               Result  = ComputeCCAPalette
             else
               begin
                 if Length(FColourPalettes.Transitions) = 0 then
                   WorkingColourPalette.SetToDefaults
                 else
                   WorkingColourPalette.PopulateFromPaletteColours(FColourPalettes);
           end;

             if Result then
               WorkingColourPalette.ComputePalette;
           end
         else
           WorkingColourPalette  = Nil;

       Except
         On e:Exception do
           SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Error: %s ', [Self.ClassName, e.Message]), slmcException);
       end;
     end;
   */

    /// <summary>
    /// Renders all subgrids in a representational style that indicates where there is data, but nothing else. This is used for large scale displays
    /// (zoomed out a lot) where meaningful detail cannot be drawn on the tile
    /// </summary>
    /// <returns></returns>
    private IBitmap RenderTileAsRepresentationalDueToScale(ISubGridTreeBitMask overallExistenceMap)
    {
      PVMDisplayerBase RepresentationalDisplayer = PVMDisplayerFactory.GetDisplayer(Mode /*, FICOptions*/);

      RepresentationalDisplayer.MapView = new MapSurface
      {
        SquareAspect = false,
        Rotation = -TileRotation + Math.PI / 2
      };

      RepresentationalDisplayer.MapView.SetBounds(NPixelsX, NPixelsY);
      RepresentationalDisplayer.MapView.SetWorldBounds(NEECoords[0].X, NEECoords[0].Y,
        NEECoords[0].X + WorldTileWidth, NEECoords[0].Y + WorldTileHeight, 0);

      // Iterate over all the bits in the subgrids drawing a rectangle for each one on the tile being rendered
      if (overallExistenceMap.ScanSubGrids(RotatedTileBoundingExtents,
        leaf =>
        {
          leaf.CalculateWorldOrigin(out double WorldOriginX, out double WorldOriginY);

          (leaf as SubGridTreeLeafBitmapSubGrid).Bits.ForEachSetBit((x, y) =>
          {
            RepresentationalDisplayer.MapView.DrawRect(WorldOriginX + (x * overallExistenceMap.CellSize),
              WorldOriginY + (y * overallExistenceMap.CellSize),
              overallExistenceMap.CellSize, overallExistenceMap.CellSize, true, RepresentColor);
          });

          return true;
        }))
      {
        return RepresentationalDisplayer.MapView.BitmapCanvas;
      }

      return null; // It did not work out...
    }

    /// <summary>
    /// Executor that implements requesting and rendering subgrid information to create the rendered tile
    /// </summary>
    /// <returns></returns>
    public IBitmap Execute()
    {
      // WorkingColourPalette  : TICDisplayPaletteBase;
      // CoordConversionResult : TCoordServiceErrorStatus;

      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}, Mode={Mode}");

      ApplicationServiceRequestStatistics.Instance.NumMapTileRequests.Increment();

      /*
     if Assigned(ASNodeImplInstance.RequestCancellations) and
        ASNodeImplInstance.RequestCancellations.IsRequestCancelled(FExternalDescriptor) then
       begin
         if VLPDSvcLocations.Debug_LogDebugRequestCancellationToFile then
           SIGLogMessage.PublishNoODS(Self, 'Request cancelled: ' + FExternalDescriptor.ToString, slmcDebug);

         ResultStatus  = asneRequestHasBeenCancelled;
         InterlockedIncrement64(ASNodeRequestStats.NumMapTileRequestsCancelled);
       Exit;
       end;

      // The governor is intended to restrict the numbers of heavy weight processes
      // such as pipelines that interact with the PC layer to request subgrids
      ScheduledWithGovernor  = ASNodeImplInstance.Governor.Schedule(FExternalDescriptor, Self, gqWMS, ResultStatus);
      if not ScheduledWithGovernor then
        Exit;
      */

      Guid RequestDescriptor = Guid.NewGuid();

      /* TODO Readd when logging available
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
                                         slmcMessage)
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
                                         slmcMessage);
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
                                         slmcMessage)
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
                                         slmcMessage);
          end;

        // Include the details of the filter with the logged tile parameters
        SIGLogMessage.PublishNoODS(Self, Format('Filter1: %s', [IfThen(FFilter1.IsNull, 'Null', FFilter1.ToString)]), slmcMessage);
        if Assigned(FFilter2) then
          SIGLogMessage.PublishNoODS(Self, Format('Filter2: %s', [IfThen(FFilter2.IsNull, 'Null', FFilter2.ToString)]), slmcMessage);
      end;
      */

      // Determine the grid (NEE) coordinates of the bottom/left, top/right WGS-84 positions
      // given the coordinate system assigned to the project. If there is no coordinate system
      // then this is where we exit

      LLHCoords = new[]
      {
        new XYZ(BLPoint.X, BLPoint.Y), // BL
        new XYZ(TRPoint.X, TRPoint.Y), // TR
        new XYZ(BLPoint.X, TRPoint.Y), // TL
        new XYZ(TRPoint.X, BLPoint.Y) // BR
      };

      if (CoordsAreGrid)
        NEECoords = LLHCoords;
      else
      {
        /* TODO: RequestCoordinateConversion
        CoordConversionResult = ASNodeImplInstance.CoordService.RequestCoordinateConversion(RequestDescriptor, DataModelID, cctLLHtoNEE, LLHCoords, EmptyStr, NEECoords);
        if (CoordConversionResult != csOK)
        {
            ResultStatus = RequestErrorStatus.FailedToConvertClientWGSCoords;
            return;
        }
        */
      }

      WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
      WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

      double dx = NEECoords[2].X - NEECoords[0].X;
      double dy = NEECoords[2].Y - NEECoords[0].Y;
      TileRotation = (Math.PI / 2) - Math.Atan2(dy, dx);

      RotatedTileBoundingExtents.SetInverted();
      foreach (XYZ xyz in NEECoords)
        RotatedTileBoundingExtents.Include(xyz.X, xyz.Y);


      /* TODO Readd when logging available
   {$IFDEF DEBUG}
    SIGLogMessage.PublishNoODS(Self,
                              Format('(%d) Tile render executing across tile: [Rotation:%.3fdeg] [BL:%.3f, %.3f, TL:%.3f, %.3f, TR:%.3f, %.3f, BR:%.3f, %.3f](%.3f x %.3f), Lat/Lon = [%.8f, %.8f, %.8f, %.8f](%.8f x %.8f)',
                                     [FDataModelID,
                                      TileRotation * (180/PI),
                                      NEECoords[0].X, NEECoords[0].Y,
                                      NEECoords[2].X, NEECoords[2].Y,
                                      NEECoords[1].X, NEECoords[1].Y,
                                      NEECoords[3].X, NEECoords[3].Y,
                                      NEECoords[1].X - NEECoords[0].X, NEECoords[1].Y - NEECoords[0].Y,
                                      FBLPoint.Lon* (180/PI), FBLPoint.Lat* (180/PI), FTRPoint.Lon* (180/PI), FTRPoint.Lat* (180/PI),
                                      (FTRPoint.Lon - FBLPoint.Lon) * (180/PI), (FTRPoint.Lat - FBLPoint.Lat) * (180/PI)]),
                              slmcDebug);
   {$ENDIF}
*/

      // Construct the renderer, configure it, and set it on its way
      //  WorkingColourPalette = Nil;

      PlanViewTileRenderer Renderer = new PlanViewTileRenderer();
      try
      {
        ISiteModel SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);
        if (SiteModel == null)
          return null;

        // Intersect the site model extents with the extents requested by the caller
        if (!SiteModel.SiteModelExtent.Intersect(RotatedTileBoundingExtents).IsValidPlanExtent)
        {
          ResultStatus = RequestErrorStatus.InvalidCoordinateRange;
            
          //RenderTransparentTile();
          return Renderer.Displayer?.MapView.RenderingFactory.CreateBitmap(NPixelsX, NPixelsY);
        }

        // Compute the override cell boundary to be used when processing cells in the subgrids
        // selected as a part of this pipeline
        // Increase cell boundary by one cell to allow for cells on the boundary that cross the boundary

        SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MinX,
          RotatedTileBoundingExtents.MinY, SiteModel.Grid.CellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
          out uint CellExtents_MinX, out uint CellExtents_MinY);
        SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MaxX,
          RotatedTileBoundingExtents.MaxY, SiteModel.Grid.CellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
          out uint CellExtents_MaxX, out uint CellExtents_MaxY);

        BoundingIntegerExtent2D CellExtents = new BoundingIntegerExtent2D((int) CellExtents_MinX, (int) CellExtents_MinY, (int) CellExtents_MaxX, (int) CellExtents_MaxY);
        CellExtents.Expand(1);

        // Create the filter set for the request execution. Only include the second filter if the mode supports it
        bool ModeRequiresTwoFilters = Mode == DisplayMode.VolumeCoverage;

        IFilterSet Filters = ModeRequiresTwoFilters ? new FilterSet(Filter1, Filter2) : new FilterSet(Filter1);

        // Construct PipelineProcessor
        IPipelineProcessor processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(
          requestDescriptor: RequestDescriptor,
          dataModelID: DataModelID,
          siteModel: SiteModel,
          gridDataType: GridDataFromModeConverter.Convert(Mode),
          response: new SubGridsPipelinedReponseBase(),
          cutFillDesignID: CutFillDesignID,
          filters: Filters,
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITask>>()(PipelineProcessorTaskStyle.PVMRendering),
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requireSurveyedSurfaceInformation: Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode) &&
                                             Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
          requestRequiresAccessToDesignFileExistenceMap: Utilities.RequestRequiresAccessToDesignFileExistenceMap(Mode /*ReferenceVolumeType*/),
          overrideSpatialCellRestriction: CellExtents
        );

        // Set the PVM rendering task parameters for progressive processing
        processor.Task.RequestDescriptor = RequestDescriptor;
        processor.Task.TRexNodeID = RequestingTRexNodeID;
        processor.Task.GridDataType = GridDataFromModeConverter.Convert(Mode);
        ((IPVMRenderingTask)processor.Task).TileRenderer = Renderer;

        // Set the spatial extents of the tile boundary rotated into the north reference frame of the cell coordinate syste to act as
        // a final restrictor of the spatial extent used to govern data requests
        processor.OverrideSpatialExtents = RotatedTileBoundingExtents;

        // Prepare the processor
        if (!processor.Build())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {SiteModel.ID}");
          return null;
        }

        // Test to see if the tile can be satisfied with a representational render indicating where
        // data is but not what it is (this is useful when the zoom level is far enough away that we
        // cannot meaningfully render the data). If the size of s subgrid is smaller than
        // the size of a pixel in the requested tile then do this. Just check the X dimension
        // as the data display is isotropic.
        if (Utilities.SubgridShouldBeRenderedAsRepresentationalDueToScale(WorldTileWidth, WorldTileHeight, NPixelsX, NPixelsY, processor.OverallExistenceMap.CellSize))
          return RenderTileAsRepresentationalDueToScale(processor.OverallExistenceMap); // There is no need to do anything else

        /* TODO - Create a scaled palette to use when rendering the data
        // Create a scaled palette to use when rendering the data
        if not CreateAndInitialiseWorkingColourPalette then
          begin
          SIGLogMessage.PublishNoODS(Self, Format('Failed to create and initialise working colour palette for data: %s in datamodel %d', [TypInfo.GetEnumName(TypeInfo(TICDisplayMode), Ord(FMode)), FDataModelID]), slmcWarning);
          Exit;
          end;
        */

        /* TODO Lift build settings not supported
        if (Filter1 != null && Filter1.AttributeFilter.HasLayerStateFilter || Filter1.AttributeFilter.LayerMethod != TFilterLayerMethod.flmNone)
            Renderer.LiftBuildSettings = ICOptions.GetLiftBuildSettings(Filter1.LayerMethod);
        else
            Renderer.LiftBuildSettings = ICOptions.LiftBuildSettings;
        */

        // Renderer.WorkingPalette = WorkingColourPalette;
        // Renderer.ReferenceVolumeType = FReferenceVolumeType;

        Renderer.IsWhollyInTermsOfGridProjection = true; // Ensure the renderer knows we are using grid projection coordinates
        Renderer.SetBounds(NEECoords[0].X, NEECoords[0].Y, WorldTileWidth, WorldTileHeight, NPixelsX, NPixelsY);
        Renderer.TileRotation = TileRotation;
        Renderer.WorldTileWidth = WorldTileWidth;
        Renderer.WorldTileHeight = WorldTileHeight;

        ResultStatus = Renderer.PerformRender(Mode, processor);

        if (processor.Response.ResultStatus == RequestErrorStatus.OK)
        {
          return Renderer.Displayer.MapView.BitmapCanvas;
        }
      }
      catch (Exception e)
      {
        Log.LogError($"Exception {e} occurred");
        ResultStatus = RequestErrorStatus.Exception;
      }

      return null;
    }
  }
}
