using System;
using System.Drawing;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Rendering.Displayers;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.CCAColorScale;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering
{
  /// <summary>
  /// Coordinates the display related activities required to produce a rendered thematic tile at a location in the world,
  /// or a required thematic layer according to filtering and other processing criteria and configuration
  /// </summary>
  public class PlanViewTileRenderer
  {
    //private static readonly ILogger Log = Logging.Logger.CreateLogger<PlanViewTileRenderer>();

    public double OriginX;
    public double OriginY;
    public double Width;
    public double Height;

    public ushort NPixelsX;
    public ushort NPixelsY;

    public PVMDisplayerBase Displayer;

    // DisplayPalettes : TICDisplayPalettes;
    // Palette : TICDisplayPaletteBase;       
    // ICOptions : TSVOICOptions;
    // LiftBuildSettings : TICLiftBuildSettings;

    // The rotation of tile in the grid coordinate space due to any defined
    // rotation on the coordinate system.
    public double TileRotation { get; set; }

    // WorldTileWidth, WorldTileHeight and contain the world size of tile being rendered
    // once any rotation of the tile has been removed
    public double WorldTileWidth = 0.0;
    public double WorldTileHeight = 0.0;

    // IsWhollyInTermsOfGridProjection determines if we can use a fixed square
    // aspect view and adjust the world coordinate bounds of the viewport to
    // accommodate the extent of the requested display area (Value = True), or
    // if the source is in terms of WGS84 lat/long where scaling and rotation
    // in the Lat/Long geodetic transform to grid coordinates needs to be
    // taken into account (Value=False)
    public bool IsWhollyInTermsOfGridProjection = false;

    // function GetWorkingPalette: TICDisplayPaletteBase;
    // procedure SetWorkingPalette(const Value: TICDisplayPaletteBase);

    private static readonly bool _debugDrawDiagonalCrossOnRenderedTilesDefault = DIContext.Obtain<IConfigurationStore>().GetValueBool("DEBUG_DRAWDIAGONALCROSS_ONRENDEREDTILES", Consts.DEBUG_DRAWDIAGONALCROSS_ONRENDEREDTILES);

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public PlanViewTileRenderer()
    {
    }

    private void PerformAnyRequiredDebugLevelDisplay()
    {
      /*
      double X, Y;
      
      if not (VLPDSvcLocations.Debug_VLPDWMSRendering_AnnotateTilesWithBoundary or
              VLPDSvcLocations.Debug_VLPDWMSRendering_AnnotateTilesWithSubgridBoundaries or
              VLPDSvcLocations.Debug_VLPDWMSRendering_AnnotateTilesWithNumberOfSubgrids) then
        Exit;

      FDisplayer.MapView.DisplaySurface.Canvas.Lock;
      try
        if VLPDSvcLocations.Debug_VLPDWMSRendering_AnnotateTilesWithSubgridBoundaries then
          with FRotatedTileBoundingExtents do
            begin
              // Draw the boundaries of all the subgrids on the tile
              X := (Trunc((MinX / (kSubGridTreeDimension * FCellSize))) - 1) * (kSubGridTreeDimension * FCellSize);
              repeat
                FDisplayer.MapView.DrawLine(X, MinY, X, MaxY, clRed);
                X := X + (kSubGridTreeDimension * FCellSize);
              until X > MaxX;

              Y := (Trunc((MinY / (kSubGridTreeDimension * FCellSize))) - 1) * (kSubGridTreeDimension * FCellSize);
              repeat
                FDisplayer.MapView.DrawLine(MinX, Y, MaxX, Y, clRed);
                Y := Y + (kSubGridTreeDimension * FCellSize);
              until Y > MaxY;
            end;

        if VLPDSvcLocations.Debug_VLPDWMSRendering_AnnotateTilesWithBoundary then
          begin
            //Draw the boundary of the tile
            FDisplayer.MapView.DrawRect(FDisplayer.MapView.OriginX, FDisplayer.MapView.OriginY,
                                        FDisplayer.MapView.WidthX, FDisplayer.MapView.WidthY,
                                        False, clBlack, bsSolid, False);
          end;

        if VLPDSvcLocations.Debug_VLPDWMSRendering_AnnotateTilesWithNumberOfSubgrids then
          begin
            // Display the number of subgrids scanned to draw the tile as a text block in the
            // center of the tile
            FDisplayer.MapView.DrawText(IntToStr(FPipeLine.OperationNode.TotalOperatedOnSubgrids),
                                        FDisplayer.MapView.CenterX, FDisplayer.MapView.CenterY,
                                        FDisplayer.MapView.DrawCanvas.Font,
                                        12 * FDisplayer.MapView.YPixelSize, pi/2, $000000);
          end;
      finally
        FDisplayer.MapView.DisplaySurface.Canvas.UnLock;
      end;
      */
    }

    //      property WorkingPalette : TICDisplayPaletteBase read GetWorkingPalette write SetWorkingPalette;
    //      property DisplayPalettes : TICDisplayPalettes read FDisplayPalettes write FDisplayPalettes;
    //      property ICOptions : TSVOICOptions read FICOptions write FICOptions;
    //      property LiftBuildSettings : TICLiftBuildSettings read FLiftBuildSettings write FLiftBuildSettings;

    /// <summary>
    /// Perform rendering activities to produce a bitmap tile
    /// </summary>
    public RequestErrorStatus PerformRender(DisplayMode mode, IPipelineProcessor processor, IPlanViewPalette colourPalette, IFilterSet filters, ILiftParameters liftParams)
    {
      // Obtain the display responsible for rendering the thematic information for this mode
      Displayer = PVMDisplayerFactory.GetDisplayer(mode /*, FICOptions*/);

      if (Displayer == null)
      {
        processor.Response.ResultStatus = RequestErrorStatus.UnsupportedDisplayType;
        return processor.Response.ResultStatus;
      }

      // Create and assign the colour pallete logic for this mode to the displayer
      if (colourPalette == null)
      {
        if (mode == DisplayMode.CCA || mode == DisplayMode.CCASummary)
        {
          Displayer.Palette = Utilities.ComputeCCAPalette(processor.SiteModel, filters.Filters[0].AttributeFilter, mode);

          if (Displayer.Palette == null)
          {
            processor.Response.ResultStatus = RequestErrorStatus.FailedToGetCCAMinimumPassesValue;
            return processor.Response.ResultStatus;
          }
        }
        else
          Displayer.Palette = PVMPaletteFactory.GetPalette(processor.SiteModel, mode, processor.SpatialExtents);
      }
      else
        Displayer.Palette = colourPalette;

      // Create the world coordinate display surface the displayer will render onto
      Displayer.MapView = new MapSurface
      {
        SquareAspect = IsWhollyInTermsOfGridProjection
      };

      // Set the world coordinate bounds of the display surface to be rendered on
      Displayer.MapView.SetBounds(NPixelsX, NPixelsY);

      if (IsWhollyInTermsOfGridProjection)
        Displayer.MapView.FitAndSetWorldBounds(OriginX, OriginY, OriginX + WorldTileWidth, OriginY + WorldTileHeight, 0);
      else
        Displayer.MapView.SetWorldBounds(OriginX, OriginY, OriginX + WorldTileWidth, OriginY + WorldTileHeight, 0);

      // Set the rotation of the displayer rendering surface to match the tile rotation due to the project calibration rotation
      // TODO - Understand why the (+ PI/2) rotation is not needed when rendering in C# bitmap contexts
      Displayer.MapView.SetRotation(-TileRotation /* + (Math.PI / 2) */);

      // Displayer.ICOptions  = ICOptions;

      // Se the skip-step area control cell selection parameters for this tile render
      processor.Pipeline.AreaControlSet = new AreaControlSet(true, Displayer.MapView.XPixelSize,
        Displayer.MapView.YPixelSize, 0, 0, 0);

      // todo PipeLine.TimeToLiveSeconds = VLPDSvcLocations.VLPDPSNode_TilePipelineTTLSeconds;
      // todo PipeLine.LiftBuildSettings  = FICOptions.GetLiftBuildSettings(FFilter1.LayerMethod);
      // todo PipeLine.NoChangeVolumeTolerance  = FICOptions.NoChangeVolumeTolerance;

      // Perform the sub grid query and processing to render the tile
      processor.Process();

      if (processor.Response.ResultStatus == RequestErrorStatus.OK)
      {
        PerformAnyRequiredDebugLevelDisplay();

        if (_debugDrawDiagonalCrossOnRenderedTilesDefault)
        {
          // Draw diagonal cross and top left corner indicators
          Displayer.MapView.DrawLine(Displayer.MapView.OriginX, Displayer.MapView.OriginY, Displayer.MapView.LimitX, Displayer.MapView.LimitY, Color.Red);
          Displayer.MapView.DrawLine(Displayer.MapView.OriginX, Displayer.MapView.LimitY, Displayer.MapView.LimitX, Displayer.MapView.OriginY, Color.Red);

          // Draw the horizontal line a little below the world coordinate 'top' of the tile to encourage the line
          // drawing algorithm not to clip it
          Displayer.MapView.DrawLine(Displayer.MapView.OriginX, Displayer.MapView.LimitY, Displayer.MapView.OriginX, Displayer.MapView.CenterY, Color.Red);
          Displayer.MapView.DrawLine(Displayer.MapView.OriginX, Displayer.MapView.LimitY - 0.01, Displayer.MapView.CenterX, Displayer.MapView.LimitY - 0.01, Color.Red);
        }
      }

      return processor.Response.ResultStatus;
    }

    /// <summary>
    /// Sets the full bounds definition for the tile to be rendered in terms of its real world coordinate
    /// origin, its real world coordinate width and height and the number of pixels for the width and height
    /// of the resulting rendered tile.
    /// </summary>
    /// <param name="AOriginX"></param>
    /// <param name="AOriginY"></param>
    /// <param name="AWidth"></param>
    /// <param name="AHeight"></param>
    /// <param name="ANPixelsX"></param>
    /// <param name="ANPixelsY"></param>
    public void SetBounds(double AOriginX, double AOriginY,
      double AWidth, double AHeight,
      ushort ANPixelsX, ushort ANPixelsY)
    {
      OriginX = AOriginX;
      OriginY = AOriginY;
      Width = AWidth;
      Height = AHeight;
      NPixelsX = ANPixelsX;
      NPixelsY = ANPixelsY;
    }
  }
}
