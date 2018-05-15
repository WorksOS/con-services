using log4net;
using System;
using System.Reflection;
using VSS.TRex.Executors.Tasks;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.Rendering.Displayers;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering
{
    public class PlanViewTileRenderer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The Raptor application service node performing the request
        /// </summary>
        string RequestingRaptorNodeID { get; set; } = string.Empty;

        public long RequestDescriptor;
        // FExternalDescriptor : TASNodeRequestDescriptor;

        public Guid DataModelID;

        public DisplayMode Mode;

        public CombinedFilter Filter1 = null;
        public CombinedFilter Filter2 = null;

        public double OriginX;
        public double OriginY;
        public double Width;
        public double Height;

        public ushort NPixelsX;
        public ushort NPixelsY;

        public PVMDisplayerBase Displayer;

        public SubGridTreeSubGridExistenceBitMask OverallExistenceMap;
        public SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap;
        public SubGridTreeSubGridExistenceBitMask DesignSubgridOverlayMap;

        // DisplayPalettes : TICDisplayPalettes;

        // Palette : TICDisplayPaletteBase;
        IPlanViewPalette Palette;

        // ICOptions : TSVOICOptions;

        private SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse> PipeLine;

        // LiftBuildSettings : TICLiftBuildSettings;

        public BoundingWorldExtent3D SpatialExtents;

        public double CellSize;

        public bool AbortedDueToTimeout;

        public int EpochCount;

        // The rotation of tile in the grid coordinate space due to any defined
        // rotation on the coordinate system.
        public double TileRotation;

        // FRotatedTileBoundingExtents is the north oriented bounding extent that
        // encloses the spatial extent of the rotated tile rectangle
        public BoundingWorldExtent3D RotatedTileBoundingExtents;

        // FWorldTileWidth, FWorldTileHeight and contain the world size of tile being rendered
        // once any rotation of the tile has been removed
        public double WorldTileWidth;
        public double WorldTileHeight;

        public bool SurveyedSurfacesExludedViaTimeFiltering;

        // FIsWhollyInTermsOfGridProjection determines if we can use a fixed square
        // aspect view and adjust the world coordinate bounds of the viewport to
        // accommodate the extent of the requested display area (Value = True), or
        // if the source is in terms of WGS84 lat/long where scaling and rotation
        // in the Lat/Long geodetic transform to grid coordinates needs to be
        // taken into account (Value=False)
        public bool IsWhollyInTermsOfGridProjection;

        // function GetWorkingPalette: TICDisplayPaletteBase;
        // procedure SetWorkingPalette(const Value: TICDisplayPaletteBase);

        private void ConfigurePipeline(out BoundingIntegerExtent2D CellExtents)
        {
            PipeLine.RequestDescriptor = RequestDescriptor;
            // PipeLine.ExternalDescriptor  = ExternalDescriptor;

            // Compute the override cell boundary to be used when processing cells in the subgrids
            // selected as a part of this pipeline
            // Increase cell boundary by one cell to allow for cells on the boundary that cross the boundary

            SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MinX, RotatedTileBoundingExtents.MinY,
                                                               CellSize, SubGridTree.DefaultIndexOriginOffset,
                                                               out uint CellExtents_MinX, out uint CellExtents_MinY);
            SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MaxX, RotatedTileBoundingExtents.MaxY,
                                                               CellSize, SubGridTree.DefaultIndexOriginOffset,
                                                               out uint CellExtents_MaxX, out uint CellExtents_MaxY);

            CellExtents = new BoundingIntegerExtent2D((int)CellExtents_MinX, (int)CellExtents_MinY, (int)CellExtents_MaxX, (int)CellExtents_MaxY);
            CellExtents.Expand(1);

            PipeLine.OverrideSpatialCellRestriction = CellExtents;
            // PipeLine.SubmissionNode.NodeDescriptor  = ASNodeImplInstance.NodeDescriptor;
            // PipeLine.SubmissionNode.RequestDescriptor  = FRequestDescriptor;
            // PipeLine.SubmissionNode.DescriptorType  = cdtWMSTile;

            PipeLine.AreaControlSet = new AreaControlSet(Displayer.MapView.XPixelSize, Displayer.MapView.YPixelSize, 0, 0, 0, true);

            // PipeLine.TimeToLiveSeconds = VLPDSvcLocations.VLPDPSNode_TilePipelineTTLSeconds;

            PipeLine.DataModelID = DataModelID;

            // PipeLine.LiftBuildSettings  = FICOptions.GetLiftBuildSettings(FFilter1.LayerMethod);

            // If summaries of compaction information (both CMV and MDP) are being displayed,
            // and the lift build settings requires all layers to be examined (so the
            // apropriate summarize top layer only flag is false), then instruct the layer
            // analysis engine to apply to restriction to the number of cell passes to use
            // to perform layer analysis (ie: all cell passes will be used).

            /* TODO...
            if (Mode == DisplayMode.CCVSummary || Mode == DisplayMode.CCVPercentSummary)
            {
                if (!PipeLine.LiftBuildSettings.CCVSummarizeTopLayerOnly)
                {
                    PipeLine.MaxNumberOfPassesToReturn = VLPDSvcLocations.VLPDASNode_MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
                }
            }

            if (Mode == DisplayMode.MDPSummary || Mode == DisplayMode.MDPPercentSummary)
            {
                if (!PipeLine.LiftBuildSettings.MDPSummarizeTopLayerOnly)
                {
                    PipeLine.MaxNumberOfPassesToReturn = VLPDSvcLocations.VLPDASNode_MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
                }
            }
            */

            PipeLine.WorldExtents = RotatedTileBoundingExtents;

            PipeLine.OverallExistenceMap = OverallExistenceMap;
            PipeLine.ProdDataExistenceMap = ProdDataExistenceMap;
            PipeLine.DesignSubgridOverlayMap = DesignSubgridOverlayMap;

            PipeLine.GridDataType = GridDataFromModeConverter.Convert(Mode);

            // Construct and assign the filter set into the pipeline
            PipeLine.FilterSet = new FilterSet(Filter2 == null ? new [] { Filter1 } : new [] { Filter1, Filter2 });

            PipeLine.IncludeSurveyedSurfaceInformation = Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode) && !SurveyedSurfacesExludedViaTimeFiltering;
            if (PipeLine.IncludeSurveyedSurfaceInformation)  // if required then check if filter turns off requirement due to filters used
            {
                PipeLine.IncludeSurveyedSurfaceInformation = Utilities.FilterRequireSurveyedSurfaceInformation(PipeLine.FilterSet);
            }

            //PipeLine.NoChangeVolumeTolerance  = FICOptions.NoChangeVolumeTolerance;
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

        protected RequestErrorStatus ExecutePipeline()
        {
            PipelinedSubGridTask PipelinedTask;

            RequestErrorStatus Result = RequestErrorStatus.Unknown;

            bool PipelineAborted = false;
            // bool ShouldAbortDueToCompletedEventSet  = false;

            try
            {
                Displayer.MapView.SetBounds(NPixelsX, NPixelsY);

                if (IsWhollyInTermsOfGridProjection)
                {
                    Displayer.MapView.FitAndSetWorldBounds(OriginX, OriginY, OriginX + WorldTileWidth, OriginY + WorldTileHeight, 0);
                }
                else
                {
                    Displayer.MapView.SetWorldBounds(OriginX, OriginY, OriginX + WorldTileWidth, OriginY + WorldTileHeight, 0);
                }

                // TODO - Understand why the (+ PI/2) rotation is not needed when rendering in C# bitmap contexts
                Displayer.MapView.SetRotation(-TileRotation /* + (Math.PI / 2) */);

                // Displayer.ICOptions  = ICOptions;

                PipelinedTask = new PVMRenderingTask(RequestDescriptor, RequestingRaptorNodeID, GridDataFromModeConverter.Convert(Mode), this);

                // ASNodeImplInstance.AsyncResponder.ASNodeResponseProcessor.ASTasks.Add(PipelinedTask);
                try
                {
                    PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(/*0, */ PipelinedTask);
                    // PipeLine = ASNodeImplInstance.SubgridPipelinePool.AcquirePipeline;

                    PipelinedTask.PipeLine = PipeLine;
                    try
                    {
                        ConfigurePipeline(out BoundingIntegerExtent2D CellExtents);

                        // EpochCount = 0
                        if (PipeLine.Initiate())
                        {
                            PipeLine.WaitForCompletion();
                        }

                        PerformAnyRequiredDebugLevelDisplay();

                        PipelineAborted = PipeLine.Aborted;

                        if (!PipeLine.Terminated && !PipeLine.Aborted)
                        {
                            Result = RequestErrorStatus.OK;
                        }
                    }
                    finally
                    {
                        // Unhook the pipeline from the task and release the pipeline back to the pool
                        // This is not needed though as subgrids will be pending in the processing 
                        // threads receiving results from the processing cluster
                        //   ASNodeImplInstance.SubgridPipelinePool.ReleasePipeline(PipeLine);
                    }
                }
                finally
                {
                    if (AbortedDueToTimeout)
                    {
                        Result = RequestErrorStatus.AbortedDueToPipelineTimeout;
                    }
                    else
                    {
                        if (PipelinedTask.IsCancelled || PipelineAborted)
                        {
                            Result = RequestErrorStatus.RequestHasBeenCancelled;
                        }

                        //  ASNodeImplInstance.AsyncResponder.ASNodeResponseProcessor.ASTasks.Remove(PipelinedTask);
                    }
                }
            }
            catch (Exception E)
            {
                
                Log.ErrorFormat("ExecutePipeline raised exception '{0}'", E);
            }

            return Result;
        }

        protected void SetDisplayerPalette()
        {
            Palette = PVMPaletteFactory.GetPallete(SiteModels.SiteModels.Instance().GetSiteModel(DataModelID), Mode, SpatialExtents);
            Displayer.Palette = Palette;
        }

        public void CreateDisplayer()
        {
            Displayer = PVMDisplayerFactory.GetDisplayer(Mode /*, FICOptions*/);
        }

        //      property WorkingPalette : TICDisplayPaletteBase read GetWorkingPalette write SetWorkingPalette;
        //      property DisplayPalettes : TICDisplayPalettes read FDisplayPalettes write FDisplayPalettes;
        //      property ICOptions : TSVOICOptions read FICOptions write FICOptions;
        //      property LiftBuildSettings : TICLiftBuildSettings read FLiftBuildSettings write FLiftBuildSettings;

        public RequestErrorStatus PerformRender()
        {
            CreateDisplayer();

            if (Displayer == null)
            {
                return RequestErrorStatus.UnsupportedDisplayType;
            }

            Displayer.MapView = new MapSurface
            {
                SquareAspect = IsWhollyInTermsOfGridProjection
            };

            SetDisplayerPalette();

            return ExecutePipeline();
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

        /// <summary>
        /// Basi constructor accepting the RaptorNodeID indicating the originating server of the request
        /// to send responses to
        /// </summary>
        /// <param name="requestingRaptorNodeID"></param>
        public PlanViewTileRenderer(string requestingRaptorNodeID)
        {
            RequestingRaptorNodeID = requestingRaptorNodeID;

            AbortedDueToTimeout = false;
            Displayer = null;
            //Palette = null;

            EpochCount = 0;
            TileRotation = 0.0;
            RotatedTileBoundingExtents = new BoundingWorldExtent3D();
            RotatedTileBoundingExtents.Clear();

            WorldTileWidth = 0;
            WorldTileHeight = 0;

            SurveyedSurfacesExludedViaTimeFiltering = false;
            IsWhollyInTermsOfGridProjection = false;
        }
    }
}
