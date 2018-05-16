using System;
using System.IO;
using VSS.TRex.Cells;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    public interface ISubGridCellSegmentPassesDataWrapper
    {
        /// <summary>
        /// The total number of cell passes present in this segment
        /// </summary>
        int SegmentPassCount { get; set; }

        /// <summary>
        /// The number of cell passes present in the cell within this subgrid segment identitifed by X and Y in 
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        uint PassCount(uint X, uint Y);

        /// <summary>
        /// Allocates a number of passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passCount"></param>
        void AllocatePasses(uint X, uint Y, uint passCount);

        /// <summary>
        /// Adds a cell pass at an optional position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="pass"></param>
        /// <param name="position"></param>
        void AddPass(uint X, uint Y, CellPass pass, int position = -1);

        /// <summary>
        /// Replaces a cell pass at a specific position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="position"></param>
        /// <param name="pass"></param>
        void ReplacePass(uint X, uint Y, int position, CellPass pass);

        /// <summary>
        /// Locates a cell pass occurring at or immediately after a given time within the passes for a specific cell within this segment.
        /// If there is not an exact match, the returned index is the location in the cell pass list where a cell pass 
        /// with the given time woule be inserted into the list to maintain correct time ordering of the cell passes in that cell.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        bool LocateTime(uint X, uint Y, DateTime time, out int index);

        /// <summary>
        /// Reads all the cell passes for this segment
        /// </summary>
        /// <param name="reader"></param>
        void Read(BinaryReader reader);

        /// <summary>
        /// Writes all cell passes for this segment
        /// </summary>
        /// <param name="writer"></param>
        void Write(BinaryWriter writer);

        /// <summary>
        /// Retrieves the Height recorded by the cell pass at the given index from the cell passes for the cell within this segment
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passNumber"></param>
        /// <returns></returns>
        float PassHeight(uint X, uint Y, uint passNumber);

        /// <summary>
        /// Retrieves the Time recorded by the cell pass at the given index from the cell passes for the cell within this segment
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passNumber"></param>
        /// <returns></returns>
        DateTime PassTime(uint X, uint Y, uint passNumber);

        /// <summary>
        /// Integrates the cell passes from two cell pass lists into a single cell pass list. Source contains the cell passes that
        /// will be integrated with the cell passes within the cell context of the caller.
        /// Only valid for mutable representations exposing this interface. 
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="sourcePasses"></param>
        /// <param name="StartIndex"></param>
        /// <param name="EndIndex"></param>
        /// <param name="AddedCount"></param>
        /// <param name="ModifiedCount"></param>
        void Integrate(uint X, uint Y, CellPass[] sourcePasses, uint StartIndex, uint EndIndex, out int AddedCount, out int ModifiedCount);

        /// <summary>
        /// Returns a full cell pass with all attributes from the cell passes within this segment for the cell identitifed by X and Y
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passIndex"></param>
        /// <returns></returns>
        CellPass Pass(uint X, uint Y, uint passIndex);

        /// <summary>
        /// An overloaded version of Pass() with the same functionality
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passNumber"></param>
        /// <returns></returns>
        CellPass ExtractCellPass(uint X, uint Y, int passNumber);

        /// <summary>
        /// Returns a full mutable version of the cell passes contained within this segment for the cell identified by X and Y
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        CellPass[] ExtractCellPasses(uint X, uint Y);

        /// <summary>
        /// Allows a caller to supply the raw cell pass information to the segment which may convert it to 
        /// it's internal representation
        /// </summary>
        /// <param name="cellPasses"></param>
        void SetState(CellPass[,][] cellPasses);

        /// <summary>
        /// Allows a caller to query the set of all cell passes in the wrapper as a subgrid array 
        /// of cell pass stacks. Warning: Not all derivates may implement this behaviour with those that
        /// do no throwing NotImplemented exceptions.
        /// </summary>
        /// <returns></returns>
        CellPass[,][] GetState();

        /// <summary>
        /// Indicates if this segment is immutable
        /// </summary>
        /// <returns></returns>
        bool IsImmutable();

        /// <summary>
        /// Calculate the total number of passes from all the cells present in this subgrid segment
        /// </summary>
        /// <param name="TotalPasses"></param>
        /// <param name="MaxPassCount"></param>
        void CalculateTotalPasses(out uint TotalPasses, out uint MaxPassCount);

        /// <summary>
        /// Calculates the time range covering all the cell passes within the given subgrid segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        void CalculateTimeRange(out DateTime startTime, out DateTime endTime);

        /// <summary>
        /// Calculates the number of passes in the segment that occur before searchTime
        /// </summary>
        /// <param name="searchTime"></param>
        /// <param name="totalPasses"></param>
        /// <param name="maxPassCount"></param>
        void CalculatePassesBeforeTime(DateTime searchTime, out uint totalPasses, out uint maxPassCount);

        /// <summary>
        /// Causes this segment to adopt all cell passes from sourceSegment where those cell passes were 
        /// recorded at or later than a specific date
        /// </summary>
        /// <param name="sourceSegment"></param>
        /// <param name="atAndAfterTime"></param>
        void AdoptCellPassesFrom(ISubGridCellSegmentPassesDataWrapper sourceSegment, DateTime atAndAfterTime);
    }
}
