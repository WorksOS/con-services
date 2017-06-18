using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    /// <summary>
    /// This is a static, immutable, version of the cell and cell passes that make up a subgrid segment
    /// </summary>
    [Serializable]
    public class SubGridCellSegmentPassesDataWrapper_Static : SubGridCellSegmentPassesDataWrapperBase, ISubGridCellSegmentPassesDataWrapper
    {
        /// <summary>
        /// CallPasses is a single collection of cell passes stored within this subgrid cell segment.
        /// </summary>
        private CellPass[] CellPasses = null;

        /// <summary>
        /// PassData is the collection of cells as present in this segment. These cells do not store their cell passes directly,
        /// instead they store references into the CellPasses array that stores all cell passes in the segment.
        /// </summary>
        public Cell_Static[,] PassData = null;

        /// <summary>
        /// Default no-arg constructor that does not instantiate any state
        /// </summary>
        public SubGridCellSegmentPassesDataWrapper_Static()
        {
        }

        /// <summary>
        /// Constructor that accepts a set of cell passes and creates the internal structures to hold them
        /// </summary>
        /// <param name="cellPassCount"></param>
        public void SetState(CellPass[,][] cellPasses)
        {
            // Ensure eny existing state is erased
            PassData = null;
            CellPasses = null;

            // Determine the total number of passes that need to be stored and create the array to hold them
            int totalPassCount = 0;

            foreach (CellPass[] passes in cellPasses)
            {
                totalPassCount += passes.Length;
            }

            CellPasses = new CellPass[totalPassCount];

            PassData = new Cell_Static[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            // Shift all the cell pass information into the private arrays, setting the cell pass offsets for 
            // each cell to reference the correct set of cell passes in the collated array.
            uint runningPassCount = 0;
            for (int i = 0; i < SubGridTree.SubGridTreeDimension; i++)
            {
                for (int j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                {
                    PassData[i, j].CellPassOffset = runningPassCount;

                    foreach (CellPass cellPass in cellPasses[i, j])
                    {
                        CellPasses[runningPassCount++] = cellPass;
                    }
                }
            }
        }

        /// <summary>
        /// Constructor that accepts a set of non static cells and creates the internal structures to hold them
        /// </summary>
        /// <param name="cellPassCount"></param>
        public SubGridCellSegmentPassesDataWrapper_Static(Cell_NonStatic[,] cells)
        {
            // Determine the total number of passes that need to be stored and create the array to hold them
            uint totalPassCount = 0;

            foreach (Cell_NonStatic cell in cells)
            {
                totalPassCount += cell.PassCount;
            }

            CellPasses = new CellPass[totalPassCount];

            PassData = new Cell_Static[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            // Shift all the cell pass information into the private arrays, setting the cell pass offsets for 
            // each cell to reference the correct set of cell passes in the collated array.
            uint runningPassCount = 0;
            for (int i = 0; i < SubGridTree.SubGridTreeDimension; i++)
            {
                for (int j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                {
                    PassData[i, j].CellPassOffset = runningPassCount;

                    foreach (CellPass pass in cells[i, j].Passes)
                    {
                        CellPasses[runningPassCount++] = pass;
                    }
                }
            }
        }

        public uint PassCount(uint X, uint Y)
        {
            return PassData[X, Y].PassCount;
        }

        public void AllocatePasses(uint X, uint Y, uint passCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public void AddPass(uint X, uint Y, CellPass pass, int position = -1)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public void ReplacePass(uint X, uint Y, int position, CellPass pass)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public CellPass ExtractCellPass(uint X, uint Y, int passNumber)
        {
            return CellPasses[PassData[X, Y].CellPassOffset + passNumber];
        }

        public bool LocateTime(uint X, uint Y, DateTime time, out int index)
        {
            return PassData[X, Y].LocateTime(CellPasses, time, out index);
        }

        public void Read(BinaryReader reader)
        {
            base.Read(reader, this);
        }

        public void Read(uint X, uint Y, BinaryReader reader)
        {
            uint lastPassCount = PassData[X, Y].CellPassOffset + PassCount(X, Y);

            for (uint cellPassIndex = PassData[X, Y].CellPassOffset; cellPassIndex < lastPassCount; cellPassIndex++)
            {
                CellPasses[cellPassIndex].Read(reader);
            }
        }

        public void Read(uint X, uint Y, uint passNumber, BinaryReader reader)
        {
            CellPasses[PassData[X, Y].CellPassOffset + passNumber].Read(reader);
        }

        public void Write(uint X, uint Y, uint passNumber, BinaryWriter writer)
        {
            CellPasses[PassData[X, Y].CellPassOffset + passNumber].Write(writer);
        }

        public void Write(BinaryWriter writer)
        {
            base.Write(writer, this);
        }

        public void Write(uint X, uint Y, BinaryWriter writer)
        {
            uint lastPassCount = PassData[X, Y].CellPassOffset + PassCount(X, Y);
            for (uint cellPassIndex = PassData[X, Y].CellPassOffset; cellPassIndex < lastPassCount; cellPassIndex++)
            {
                CellPasses[cellPassIndex].Write(writer);
            }
        }

        public float PassHeight(uint X, uint Y, uint passNumber)
        {
            return CellPasses[PassData[X, Y].CellPassOffset + passNumber].Height;
        }

        public DateTime PassTime(uint X, uint Y, uint passNumber)
        {
            return CellPasses[PassData[X, Y].CellPassOffset + passNumber].Time;
        }

        public void Integrate(uint X, uint Y, Cell_NonStatic source, uint StartIndex, uint EndIndex, out int AddedCount, out int ModifiedCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public Cell_NonStatic Cell(uint X, uint Y)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public CellPass Pass(uint X, uint Y, uint passIndex)
        {
            return CellPasses[PassData[X, Y].CellPassOffset + passIndex];
        }
    }
}
