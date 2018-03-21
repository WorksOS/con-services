using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Velociraptor.DesignProfiling;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Designs.Storage;
using VSS.VisionLink.Raptor.Services.Designs;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;

namespace VSS.VisionLink.Raptor.Analytics.Aggregators
{
    /// <summary>
    /// Implements the specific business rules for calculating a cut fill summary
    /// </summary>
    public class CutFillAggregator : AggregatorBase
    {
        /// <summary>
        /// The array of height offsets representing the cut and fill bands of the cut-fill isopac surface being analysed
        /// </summary>
        public Double[] Offsets { get; set; }

        /// <summary>
        /// The set of counters relevant to the supplied cut fill offsets
        /// </summary>
        public long[] Counts { get; set; }

        /// <summary>
        /// The design to be used for comparison against the production data surface 
        /// </summary>
//        public Design CutFillDesign { get; set; } = null;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public CutFillAggregator()
        {
            Counts = new long[7];
        }

        /// <summary>
        /// Determines which cut/fill band to allocate the height value for a cell
        /// </summary>
        /// <param name="value"></param>
        void IncrementCountOfCutFillTransition(Double value)
        {
            // Works out what percentage of cutfill map colours are used
            // always 7 elements in array and assumes grade is set at zero
            // eg: 0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5
            if (value == 0)  // on grade
                Counts[3]++;
            else if (value > 0) // Cut
            {
                if (value >= Offsets[0])
                {
                    Counts[0]++;
                    return;
                }
                for (int I = 0; I < 3; I++)
                {
                    if (value >= Offsets[I + 1] && value < Offsets[I])
                    {
                        Counts[I + 1]++;
                        break;
                    }
                }
                // should not get past this point
            }
            else  // must be fill
            {
                if (value <= Offsets[6])
                {
                    Counts[6]++;
                    return;
                }
                for (int I = 3; I < 6; I++)
                {
                    if (value >= Offsets[I + 1] && value < Offsets[I])
                    {
                        Counts[I]++;
                        break;
                    }
                }
                // should not get past this point
            }
        }

/*
        /// <summary>
        /// Converts a height subgrid to a cut fill isopach subgrid with respect to a design surface
        /// Note: This will be obviated when direct cutfill subgrid requests are handled at the subgrid processign engine level.
        /// </summary>
        /// <param name="SubGrid"></param>
        /// <returns></returns>
        private bool ConvertElevationSubgridToCutFill(IClientLeafSubGrid SubGrid)
        {
            //ClientHeightLeafSubGrid ElevationSubgrid = null;
            ClientHeightLeafSubGrid DesignElevations = null;

            //  SIGLogMessage.PublishNoODS(Self, 'Render(ProcessTransferredSubgridResponse): Converting height to cut/fill', slmcMessage);
            try
            {
                if (CutFillDesign == null)
                {
                    // TODO Include when loggin available
                    // SIGLogMessage.PublishNoODS(Self, 'Render(ProcessTransferredSubgridResponse): Converting height to cut/fill: No design supplied, exiting', slmcError);
                    return false;
                }

                DesignProfilerRequestResult ProfilerRequestResult = DesignProfilerRequestResult.UnknownError;
                ClientHeightLeafSubGrid ElevationSubgrid = SubGrid as ClientHeightLeafSubGrid;

                if (CutFillDesign?.GetDesignHeights(SiteModelID, SubGrid.OriginAsCellAddress(), SubGrid.CellSize, out DesignElevations, out ProfilerRequestResult) == false)
                {
                    if (ProfilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
                    {
                        // TODO readd when logging available
                        //SIGLogMessage.PublishNoODS(Self, Format('Design profiler subgrid elevation request for %s failed with error %d', [BaseScanSubGrid.OriginAsCellAddress.AsText, Ord(ProfilerRequestResult)]), slmcError);
                        return false;
                    }
                }

                // Compare the design elevations in the requested design elevation patch against the elevation subgrid we have been passed
                SubGridUtilities.SubGridDimensionalIterator((I, J) =>
                {
                    if (ElevationSubgrid.Cells[I, J] != Consts.NullHeight)
                    {
                        if (DesignElevations.Cells[I, J] != Consts.NullHeight)
                        {
                            ElevationSubgrid.Cells[I, J] = ElevationSubgrid.Cells[I, J] - DesignElevations.Cells[I, J];
                        }
                        else
                        {
                            ElevationSubgrid.Cells[I, J] = Consts.NullHeight;
                        }
                    }
                });

                return true;
            }
            catch
            {
                // TODO Add when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('Render(ProcessTransferredSubgridResponse CutFill): Exception ''%s''', [E.Message]), slmcException);
            }

            return false;
        }
*/

        /// <summary>
        /// Processes an elevation subgrid into a cut fill isopach and calculate the counts of cells where the cut fill
        /// height fits into the requested bands
        /// </summary>
        /// <param name="subGrids"></param>
        public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
        {
            // Works out the percentage each colour on the map represents
            ClientHeightLeafSubGrid HeightSubGrid = subGrids[0][0] as ClientHeightLeafSubGrid;
            Single HeightValue;

/*
 *if (!ConvertElevationSubgridToCutFill(HeightSubGrid))
            {
                // TODO Add when logging available
                // SIGLogMessage.Publish(Self, 'Supplied subgrid result could not be converted to a CutFill grid', slmcError);
                return;
            }
*/
            // loop through array. Note processing is accumulative so values may already hold values
            SubGridUtilities.SubGridDimensionalIterator((I, J) =>
                            {
                                HeightValue = HeightSubGrid.Cells[I, J];
                                if (HeightValue != Consts.NullHeight) // is there a value to test
                                {
                                    SummaryCellsScanned++;
                                    IncrementCountOfCutFillTransition(HeightValue);
                                }
                            });
        }
    }
}
