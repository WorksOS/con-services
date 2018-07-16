using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.Analytics.CutFillStatistics
{
    /// <summary>
    /// Implements the specific business rules for calculating a cut fill summary
    /// </summary>
    public class CutFillAggregator : SummaryDataAggregator
    {
        /// <summary>
        /// The array of height offsets representing the cut and fill bands of the cut-fill isopac surface being analysed
        /// </summary>
        public double[] Offsets { get; set; }

        /// <summary>
        /// The set of counters relevant to the supplied cut fill offsets
        /// </summary>
        public long[] Counts { get; set; }

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
        private void IncrementCountOfCutFillTransition(double value)
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

        /// <summary>
        /// Processes an elevation subgrid into a cut fill isopach and calculate the counts of cells where the cut fill
        /// height fits into the requested bands
        /// </summary>
        /// <param name="subGrids"></param>
        public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
        {
            base.ProcessSubgridResult(subGrids);

            // Works out the percentage each colour on the map represents

            if (!(subGrids[0][0] is ClientHeightLeafSubGrid SubGrid))
                return;

            SubGridUtilities.SubGridDimensionalIterator((I, J) =>
            {
                float Value = SubGrid.Cells[I, J];
                if (Value != Consts.NullHeight) // is there a value to test
                {
                    SummaryCellsScanned++;
                    IncrementCountOfCutFillTransition(Value);
                }
            });
        }
    }
}
