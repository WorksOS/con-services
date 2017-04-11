using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    /// <summary>
    /// Interface for the iterator responsible for iterating through cell passes within a cell within segments within a subgrid
    /// </summary>
    public interface ISubGridSegmentCellPassIterator
    {
        byte CellX { get; }
        byte CellY { get; }
        int MaxNumberOfPassesToReturn { get; set; }
        ISubGridSegmentIterator SegmentIterator { get; set; }

        bool GetNextCellPass(ref CellPass CellPass);
        void Initialise();
        bool MayHaveMoreFilterableCellPasses();
        void SetCellCoordinatesInSubgrid(byte cellX, byte cellY);
        void SetIteratorElevationRange(double minElevation, double maxElevation);
        void SetTimeRange(bool hasTimeFilter, DateTime iteratorStartTime, DateTime iteratorEndTime);
    }
}
