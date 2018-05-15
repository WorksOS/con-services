using System;
using System.Diagnostics;
using System.Drawing;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
    public class ProductionPVMDisplayerBase
    {
        public const int kMaxStepSize = 10000;

        // Various quantities useful when displaying a subgrid full of grid data
        protected int StepX;
        protected int StepY;

        protected int StepXWorld;
        protected int StepYWorld;

        protected double StepXIncrement;
        protected double StepYIncrement;
        protected double StepXIncrementOverTwo;
        protected double StepYIncrementOverTwo;

        // Various quantities usefule when iterating across cells in a subgrid and drawing them

        protected int north_row, east_col;
        protected double CurrentNorth, CurrentEast;

        protected double _CellSize;
        protected double _OneThirdCellSize;
        protected double _HalfCellSize;
        protected double _TwoThirdsCellSize;

        // AccumulatingScanLine is a flag idicating we are accumulating cells together
        // to for a scan line of cells that we will display in one hit
        protected bool FAccumulatingScanLine;

        // FCellStripStartX and FCellStripEndX record the start and end of the strip we are displaying
        protected double FCellStripStartX;
        protected double FCellStripEndX;

        // FCellStripColour records the colour of the strip of cells we will draw
        protected Color FCellStripColour;

        // OriginX/y and LimitX/Y denote the extents of the physical world area covered by
        // the display context being drawn into
        protected double FOriginX, FOriginY, FLimitX, FLimitY;

        // FICOptions is a transient reference an IC options object to be used while rendering
        //      FICOptions : TSVOICOptions;

        protected bool FDisplayParametersCalculated;

        // FSubgridResult is a reference to the raw subgrid result returned from the PS layer
//        SubGridTreeLeafSubGridBaseResult SubgridResult; // : TICSubGridTreeLeafSubGridBaseResult;

        protected bool FHasRenderedSubgrid;

        private void CalculateDisplayParameters()
        {
            // Set the cell size for displaying the grid. If we will be processing
            // representative grids then set _CellSize to be the size of a leaf
            // subgrid in the sub grid tree
            _OneThirdCellSize = _CellSize * (1 / 3.0);
            _HalfCellSize = _CellSize / 2.0;
            _TwoThirdsCellSize = _CellSize * (2 / 3.0);

            double StepsPerPixelX = MapView.XPixelSize / _CellSize;
            double StepsPerPixelY = MapView.YPixelSize / _CellSize;

            StepX = Math.Min(kMaxStepSize, Math.Max(1, (int)Math.Truncate(StepsPerPixelX)));
            StepY = Math.Min(kMaxStepSize, Math.Max(1, (int)Math.Truncate(StepsPerPixelY)));

            StepXIncrement = StepX * _CellSize;
            StepYIncrement = StepY * _CellSize;

            StepXIncrementOverTwo = StepXIncrement / 2;
            StepYIncrementOverTwo = StepYIncrement / 2;
        }

        protected virtual bool DoRenderSubGrid(ISubGrid SubGrid)
        {
            bool DrawCellStrips;

            // Draw the cells in the grid in stripes, starting from the southern most
            // row in the grid and progressing from the western end to the eastern end
            // (ie: bottom to top, left to right)

            // See if this display supports cell strip rendering

            DrawCellStrips = SupportsCellStripRendering();

            // Calculate the world coordinate location of the origin (bottom left corner)
            // of this subgrid
            SubGrid.CalculateWorldOrigin(out double SubGridWorldOriginX, out double SubGridWorldOriginY);

            // Skip-Iterate through the cells drawing them in strips

            double Temp = SubGridWorldOriginY / StepYIncrement;
            CurrentNorth = (Math.Truncate(Temp) * StepYIncrement) - StepYIncrementOverTwo;
            north_row = (int)Math.Floor((CurrentNorth - SubGridWorldOriginY) / _CellSize);

            while (north_row < 0)
            {
                north_row += StepY;
                CurrentNorth += StepYIncrement;
            }

            while (north_row < SubGridTree.SubGridTreeDimension)
            {
                Temp = SubGridWorldOriginX / StepXIncrement;
                CurrentEast = (Math.Truncate(Temp) * StepXIncrement) + StepXIncrementOverTwo;
                east_col = (int)Math.Floor((CurrentEast - SubGridWorldOriginX) / _CellSize);

                while (east_col < 0)
                {
                    east_col += StepX;
                    CurrentEast += StepXIncrement;
                }

                if (DrawCellStrips)
                {
                    DoStartRowScan();
                }

                while (east_col < SubGridTree.SubGridTreeDimension)
                {
                    if (DrawCellStrips)
                    {
                        DoAccumulateStrip();
                    }
                    else
                    {
                        DoRenderCell();
                    }

                    CurrentEast += StepXIncrement;
                    east_col += StepX;
                }

                if (DrawCellStrips)
                {
                    DoEndRowScan();
                }

                CurrentNorth += StepYIncrement;
                north_row += StepY;
            }

            return true;
        }

        protected virtual void DoRenderCell()
        {
            Color Colour = DoGetDisplayColour();

            if (Colour != Color.Empty)
            {
                MapView.DrawRect(CurrentEast, CurrentNorth,
                                 _CellSize, _CellSize, true, Colour);
            }
        }

        // SupportsCellStripRendering enables a displayer to advertise is it capable
        // of rendering cell information in strips
        protected virtual bool SupportsCellStripRendering() => false;

        // DoGetDisplayColour queries the data at the current cell location and
        // determines the colour that should be displayed there. If there is no value
        // that should be displayed there (ie: it is <Null>, then the function returns
        // clnone as the colour).
        protected virtual Color DoGetDisplayColour()
        {
            // No behaviour in base class for this message
            Debug.Assert(false, "TSVOProductionPVMDisplayerBase.DoGetDisplayColour should never be called");

            return Color.Empty;
        }

        protected void DoStartRowScan() => FAccumulatingScanLine = false;

        protected void DoEndRowScan()
        {
            if (FAccumulatingScanLine)
            {
                DoRenderStrip();
            }
        }

        protected void DoAccumulateStrip()
        {
            Color DisplayColour = DoGetDisplayColour();

            if (DisplayColour != Color.Empty) // There's something to draw
            {
                // Set the end of the strip to current east
                FCellStripEndX = CurrentEast;

                if (!FAccumulatingScanLine) // We should start accumulating one
                {
                    FAccumulatingScanLine = true;
                    FCellStripColour = DisplayColour;
                    FCellStripStartX = CurrentEast;
                }
                else // ... We're already accumulating one, we might need to draw it and start again
                {
                    if (FCellStripColour != DisplayColour)
                    {
                        DoRenderStrip();

                        FAccumulatingScanLine = true;
                        FCellStripColour = DisplayColour;
                        FCellStripStartX = CurrentEast;
                    }
                }
            }
            else // The cell should not be drawn
            {
                if (FAccumulatingScanLine) // We have accumulated something that should be drawn
                {
                    DoRenderStrip();
                }
            }
        }

        protected void DoRenderStrip()
        {
            if (!FAccumulatingScanLine)
            {
                return;
            }

            if (FCellStripColour == Color.Empty)
            {
                return;
            }

            MapView.DrawRect(FCellStripStartX - StepXIncrementOverTwo,
                             CurrentNorth - StepYIncrementOverTwo,
                             (FCellStripEndX - FCellStripStartX) + StepXIncrement,
                             StepYIncrement,
                             true,
                             FCellStripColour);

            FAccumulatingScanLine = false;
        }

      public double CellSize { get { return _CellSize; } set { _CellSize = value; } }

        //      property ICOptions : TSVOICOptions read FICOptions write FICOptions;

        public MapSurface MapView { get; set; }
        public bool HasRenderedSubgrid { get; set; } = false;

        public ProductionPVMDisplayerBase()
        {

        }

      public bool RenderSubGrid(SubGridTreeLeafSubGridBaseResult subGridResult)
        {
//            SubgridResult = subGridResult;

            if (!(subGridResult.SubGrid is IClientLeafSubGrid))
            {
                // TODO Readd when logging available
                //SIGLogMessage.Publish(Self, Format('Subgrid type %s does not derive from TICSubGridTreeLeafSubGridBase', [FSubgridResult.Subgrid.ClassName]), slmcMessage);
                return false;
            }

            if (!FDisplayParametersCalculated)
            {
                _CellSize = subGridResult.SubGrid.CellSize; //TICSubGridTreeLeafSubGridBase(FSubgridResult.Subgrid).CellSize;
                CalculateDisplayParameters();
                FDisplayParametersCalculated = true;
            }

            FHasRenderedSubgrid = true;

            return DoRenderSubGrid(subGridResult.SubGrid);
        }

        public bool RenderSubGrid(IClientLeafSubGrid ClientSubGrid)
        {
            if (ClientSubGrid == null)
            {
                return true;
            }

            if (!FDisplayParametersCalculated)
            {
                _CellSize = ClientSubGrid.CellSize;
                CalculateDisplayParameters();
                FDisplayParametersCalculated = true;
            }

            FHasRenderedSubgrid = true;

            return DoRenderSubGrid(ClientSubGrid);
        }
    }
}
