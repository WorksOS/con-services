using System;
using System.Drawing;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Models;
using VSS.TRex.DI;
using VSS.TRex.DataSmoothing;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Rendering.Displayers;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering
{
  /// <summary>
  /// Coordinates the display related activities required to produce a rendered thematic tile at a location in the world,
  /// or a required thematic layer according to filtering and other processing criteria and configuration
  /// </summary>
  public class PlanViewTileRenderer : IDisposable
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<PlanViewTileRenderer>();

    public double OriginX;
    public double OriginY;
    public double Width;
    public double Height;

    public ushort NPixelsX;
    public ushort NPixelsY;

    public ProductionPVMDisplayer Displayer;

    // DisplayPalettes : TICDisplayPalettes;
    // Palette : TICDisplayPaletteBase;       
    // ICOptions : TSVOICOptions;

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

    private static readonly bool DebugDrawDiagonalCrossOnRenderedTilesDefault = DIContext.Obtain<IConfigurationStore>().GetValueBool("DEBUG_DRAWDIAGONALCROSS_ONRENDEREDTILES", Common.Consts.DEBUG_DRAWDIAGONALCROSS_ONRENDEREDTILES);

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

      // Create and assign the colour palette logic for this mode to the displayer
      if (colourPalette == null)
      {
        if (mode == DisplayMode.CCA || mode == DisplayMode.CCASummary)
        {
          Displayer.SetPalette(Utilities.ComputeCCAPalette(processor.SiteModel, filters.Filters[0].AttributeFilter, mode));

          if (Displayer.GetPalette() == null)
          {
            processor.Response.ResultStatus = RequestErrorStatus.FailedToGetCCAMinimumPassesValue;
            return processor.Response.ResultStatus;
          }
        }
        else
          Displayer.SetPalette(PVMPaletteFactory.GetPalette(processor.SiteModel, mode, processor.SpatialExtents));
      }
      else
        Displayer.SetPalette(colourPalette);

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

      // Provide data smoothing support to the displayer for the rendering operation being performed
      ((IProductionPVMConsistentDisplayer) Displayer).DataSmoother = DIContext.Obtain<Func<DisplayMode, IDataSmoother>>()(mode); 

      // Set the rotation of the displayer rendering surface to match the tile rotation due to the project calibration rotation
      // TODO - Understand why the (+ PI/2) rotation is not needed when rendering in C# bitmap contexts
      Displayer.MapView.SetRotation(-TileRotation /* + (Math.PI / 2) */);

      // Construct the PVM task accumulator for the PVM rendering task to contain the values to be rendered
      // We manage this here because the accumulator context relates to the query spatial bounds, not the rendered tile bounds
      // The acumulator is instructed to created a context covering the OverrideSpatialExtents context from the processor (which
      // will represent the bounding extent of data required due to any tile rotation), and covered by a matching (possibly larger) grid 
      // of cells to the mapview grid of pixels

      var smoother = (Displayer as IProductionPVMConsistentDisplayer)?.DataSmoother;

      var valueStoreCellSizeX = Displayer.MapView.XPixelSize > processor.SiteModel.CellSize ? Displayer.MapView.XPixelSize : processor.SiteModel.CellSize;
      var valueStoreCellSizeY = Displayer.MapView.YPixelSize > processor.SiteModel.CellSize ? Displayer.MapView.YPixelSize : processor.SiteModel.CellSize;

      var mapViewCellsX = (int)Math.Truncate(Displayer.MapView.WidthX / valueStoreCellSizeX) + 1;
      var mapViewCellsY = (int)Math.Truncate(Displayer.MapView.WidthY / valueStoreCellSizeY) + 1;

      var borderAdjustmentCells = 2 * smoother?.AdditionalBorderSize ?? 0;
      var extentAdjumentSizeX = smoother?.AdditionalBorderSize * valueStoreCellSizeX ?? 0;
      var extentAdjumentSizeY = smoother?.AdditionalBorderSize * valueStoreCellSizeY ?? 0;

      ((IPVMRenderingTask)processor.Task).Accumulator = ((IProductionPVMConsistentDisplayer)Displayer).GetPVMTaskAccumulator(
        valueStoreCellSizeX, valueStoreCellSizeY,
        mapViewCellsX + borderAdjustmentCells,
        mapViewCellsY + borderAdjustmentCells,
        Displayer.MapView.OriginX - extentAdjumentSizeX,
        Displayer.MapView.OriginY - extentAdjumentSizeY,
        Displayer.MapView.WidthX + extentAdjumentSizeX,
        Displayer.MapView.WidthY + extentAdjumentSizeY,
        processor.SiteModel.CellSize
      );

      // Displayer.ICOptions  = ICOptions;

      // Se the skip-step area control cell selection parameters for this tile render
      processor.Pipeline.AreaControlSet = new AreaControlSet(true, Displayer.MapView.XPixelSize,
        Displayer.MapView.YPixelSize, 0, 0, 0);

      // todo PipeLine.TimeToLiveSeconds = VLPDSvcLocations.VLPDPSNode_TilePipelineTTLSeconds;
      processor.Pipeline.LiftParams = liftParams;
      // todo PipeLine.NoChangeVolumeTolerance  = FICOptions.NoChangeVolumeTolerance;

      // Perform the sub grid query and processing to render the tile
      processor.Process();

      if (processor.Response.ResultStatus == RequestErrorStatus.OK)
      {
        // Render the collection of data in the aggregator
        (Displayer as IProductionPVMConsistentDisplayer)?.PerformConsistentRender(Log);

        PerformAnyRequiredDebugLevelDisplay();

        if (DebugDrawDiagonalCrossOnRenderedTilesDefault)
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
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="nPixelsX"></param>
    /// <param name="nPixelsY"></param>
    public void SetBounds(double originX, double originY,
      double width, double height,
      ushort nPixelsX, ushort nPixelsY)
    {
      OriginX = originX;
      OriginY = originY;
      Width = width;
      Height = height;
      NPixelsX = nPixelsX;
      NPixelsY = nPixelsY;
    }

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          if (Displayer != null)
          {
            Displayer.SetPalette(null);
            Displayer.Dispose();
            Displayer = null;
          }
        }

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}
