using System;
using System.Collections;
using System.IO;
using VSS.TRex.Cells;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    public interface ISubGridCellSegmentPassesDataWrapper : IDisposable
    {
        /// <summary>
        /// The total number of cell passes present in this segment
        /// </summary>
        int SegmentPassCount { get; set; }

        /// <summary>
        /// The number of cell passes present in the cell within this sub grid segment identified by X and Y in 
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        int PassCount(int X, int Y);

        /// <summary>
        /// Reduces the number of passes in the cell to newCount by preserving the first
        /// 'newCount' cell passes in the cell and retiring the remainder.
        /// If newCount is larger than the actual count an ArgumentException is thrown
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="newCount"></param>
        void TrimPassCount(int X, int Y, int newCount);

        /// <summary>
        /// Allocates a number of passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passCount"></param>
        void AllocatePasses(int X, int Y, int passCount);

        /// <summary>
        /// Adds a cell pass at an optional position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="pass"></param>
        void AddPass(int X, int Y, CellPass pass);

        /// <summary>
        /// Replaces a cell pass at a specific position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="position"></param>
        /// <param name="pass"></param>
        void ReplacePass(int X, int Y, int position, CellPass pass);

        /// <summary>
        /// Removes a cell pass at a specific position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="position"></param>
        void RemovePass(int X, int Y, int position);

        /// <summary>
        /// Locates a cell pass occurring at or immediately after a given time within the passes for a specific cell within this segment.
        /// If there is not an exact match, the returned index is the location in the cell pass list where a cell pass 
        /// with the given time would be inserted into the list to maintain correct time ordering of the cell passes in that cell.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        bool LocateTime(int X, int Y, DateTime time, out int index);

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
        float PassHeight(int X, int Y, int passNumber);

        /// <summary>
        /// Retrieves the Time recorded by the cell pass at the given index from the cell passes for the cell within this segment
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passNumber"></param>
        /// <returns></returns>
        DateTime PassTime(int X, int Y, int passNumber);

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
        void Integrate(int X, int Y, Cell_NonStatic sourcePasses, int StartIndex, int EndIndex, out int AddedCount, out int ModifiedCount);

        /// <summary>
        /// Returns a full cell pass with all attributes from the cell passes within this segment for the cell identified by X and Y
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passIndex"></param>
        /// <returns></returns>
        CellPass Pass(int X, int Y, int passIndex);

        /// <summary>
        /// An overloaded version of Pass() with the same functionality
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passNumber"></param>
        /// <returns></returns>
        CellPass ExtractCellPass(int X, int Y, int passNumber);

        /// <summary>
        /// Returns a full mutable version of the cell passes contained within this segment for the cell identified by X and Y
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        Cell_NonStatic ExtractCellPasses(int X, int Y);

        /// <summary>
        /// Replaces the collection of passes at location (x, y) with the provided set of cell passes
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="cellPasses"></param>
        void ReplacePasses(int X, int Y, CellPass[] cellPasses, int cellPassCount);

        /// <summary>
        /// Allows a caller to supply the raw cell pass information to the segment which may convert it to 
        /// it's internal representation
        /// </summary>
        /// <param name="cellPasses"></param>
        void SetState(Cell_NonStatic[,] cellPasses);

        /// <summary>
        /// Allows a caller to query the set of all cell passes in the wrapper as a sub grid array 
        /// of cell pass stacks. Warning: Not all derivatives may implement this behaviour with those that
        /// do no throwing NotImplemented exceptions.
        /// </summary>
        /// <returns></returns>
        Cell_NonStatic[,] GetState();

        /// <summary>
        /// Indicates if this segment is immutable
        /// </summary>
        /// <returns></returns>
        bool IsImmutable();

        /// <summary>
        /// Calculate the total number of passes from all the cells present in this sub grid segment
        /// </summary>
        /// <param name="totalPasses"></param>
        /// <param name="minPassCount"></param>
        /// <param name="maxPassCount"></param>
        void CalculateTotalPasses(out int totalPasses, out int minPassCount, out int maxPassCount);

        /// <summary>
        /// Calculates the time range covering all the cell passes within the given sub grid segment
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        void CalculateTimeRange(out DateTime startTime, out DateTime endTime);

        /// <summary>
        /// Calculates the number of passes in the segment that occur before searchTime
        /// </summary>
        /// <param name="searchTime"></param>
        /// <param name="totalPasses"></param>
        /// <param name="maxPassCount"></param>
        void CalculatePassesBeforeTime(DateTime searchTime, out int totalPasses, out int maxPassCount);

        /// <summary>
        /// Causes this segment to adopt all cell passes from sourceSegment where those cell passes were 
        /// recorded at or later than a specific date
        /// </summary>
        /// <param name="sourceSegment"></param>
        /// <param name="atAndAfterTime"></param>
        void AdoptCellPassesFrom(ISubGridCellSegmentPassesDataWrapper sourceSegment, DateTime atAndAfterTime);

        /// <summary>
        /// Returns any known machine ID set related to the set of passes this wrapper is responsible for
        /// </summary>
        /// <returns></returns>
        BitArray GetMachineIDSet();

        /// <summary>
        /// Sets the internal machine ID for the cell pass identified by x & y spatial location and passNumber.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passNumber"></param>
        /// <param name="internalMachineID"></param>
        void SetInternalMachineID(int X, int Y, int passNumber, short internalMachineID);

        /// <summary>
        /// Sets the internal machine ID for all cell passes within the segment to the provided ID.
        /// </summary>
        /// <param name="internalMachineIndex"></param>
        /// <param name="numModifiedPasses"></param>
        void SetAllInternalMachineIDs(short internalMachineIndex, out long numModifiedPasses);

        /// <summary>
        /// If the elevation range of the elevations stored in this segment is known then return it,
        /// return null min and max elevations otherwise.
        /// </summary>
        /// <param name="minElev"></param>
        /// <param name="maxElev"></param>
        void GetSegmentElevationRange(out double minElev, out double maxElev);

        /// <summary>
        /// Does this segment have allocated pass data available
        /// </summary>
        /// <returns></returns>
        bool HasPassData();
    }
}
