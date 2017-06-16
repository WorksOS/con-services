using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public interface ISubGridCellSegmentPassesDataWrapper
    {
        int SegmentPassCount { get; set; }

        uint PassCount(uint X, uint Y);

        void AllocatePasses(uint X, uint Y, uint passCount);
        void AddPass(uint X, uint Y, CellPass pass, int position = -1);
        void ReplacePass(uint X, uint Y, int position, CellPass pass);

        bool LocateTime(uint X, uint Y, DateTime time, out int index);

        CellPass ExtractCellPass(uint X, uint Y, int passNumber);

        void Read(BinaryReader reader);

        void Read(uint X, uint Y, BinaryReader reader);

        void Read(uint X, uint Y, uint passNumber, BinaryReader reader);

        void Write(BinaryWriter writer);

        void Write(uint X, uint Y, BinaryWriter writer);
        void Write(uint X, uint Y, uint passNumber, BinaryWriter writer);

        float PassHeight(uint X, uint Y, uint passNumber);
        DateTime PassTime(uint X, uint Y, uint passNumber);

        void Integrate(uint X, uint Y, Cell_NonStatic source, uint StartIndex, uint EndIndex, out int AddedCount, out int ModifiedCount);

        CellPass Pass(uint X, uint Y, uint passIndex);
        Cell_NonStatic Cell(uint X, uint Y);
    }

    [Serializable]
    public class SubGridCellSegmentPassesDataWrapperBase
    {
        public int SegmentPassCount { get; set; } = 0;

        private void CalculateTotalPasses(ref uint TotalPasses, ref uint MaxPassCount, ISubGridCellSegmentPassesDataWrapper target)
        {
            uint ThePassCount;

            TotalPasses = 0;
            MaxPassCount = 0;

            for (uint i = 0; i < SubGridTree.SubGridTreeDimension; i++)
            {
                for (uint j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                {
                    ThePassCount = target.PassCount(i, j);

                    if (ThePassCount > MaxPassCount)
                    {
                        MaxPassCount = ThePassCount;
                    }

                    TotalPasses += ThePassCount;
                }
            }
        }

        public void Read(BinaryReader reader, ISubGridCellSegmentPassesDataWrapper target)
        {
            int TotalPasses = reader.ReadInt32();
            int MaxPassCount = reader.ReadInt32();

            int[,] PassCounts = new int[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            int PassCounts_Size = PassCountSize.Calculate(MaxPassCount);

            for (int i = 0; i < SubGridTree.SubGridTreeDimension; i++)
            {
                for (int j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                {
                    switch (PassCounts_Size)
                    {
                        case 1: PassCounts[i, j] = reader.ReadByte(); break;
                        case 2: PassCounts[i, j] = reader.ReadInt16(); break;
                        case 3: PassCounts[i, j] = reader.ReadInt32(); break;
                        default:
                            throw new InvalidDataException(String.Format("Unknown PassCounts_Size {0}", PassCounts_Size));
                    }
                }
            }

            // Read all the cells from the stream
            for (uint i = 0; i < SubGridTree.SubGridTreeDimension; i++)
            {
                for (uint j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                {
                    int PassCount_ = PassCounts[i, j];

                    if (PassCounts[i, j] > 0)
                    {
                        // TODO: Revisit static cell pass support for reading contexts
                        target.AllocatePasses(i, j, (uint)PassCounts[i, j]);
                        target.Read(i, j, reader);

                        SegmentPassCount += PassCount_;
                    };
                }
            }
        }

        public void Write(BinaryWriter writer, ISubGridCellSegmentPassesDataWrapper target)
        {
            uint TotalPasses = 0;
            uint MaxPassCount = 0;
            CalculateTotalPasses(ref TotalPasses, ref MaxPassCount, target);

            writer.Write(TotalPasses);
            writer.Write(MaxPassCount);

            int PassCounts_Size = PassCountSize.Calculate((int)MaxPassCount);

            // Read all the cells from the stream
            for (uint i = 0; i < SubGridTree.SubGridTreeDimension; i++)
            {
                for (uint j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                {
                    switch (PassCounts_Size)
                    {
                        case 1: writer.Write((byte)target.PassCount(i, j)); break;
                        case 2: writer.Write((ushort)target.PassCount(i, j)); break;
                        case 3: writer.Write((int)target.PassCount(i, j)); break;
                        default:
                            throw new InvalidDataException(String.Format("Unknown PassCounts_Size {0}", PassCounts_Size));
                    }
                }
            }

            // write all the cells to the stream
            for (uint i = 0; i < SubGridTree.SubGridTreeDimension; i++)
            {
                for (uint j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                {
                    target.Write(i, j, writer);
                }
            }
        }

        public SubGridCellSegmentPassesDataWrapperBase()
        {
        }
    }

    [Serializable]
    public class SubGridCellSegmentPassesDataWrapper_NonStatic : SubGridCellSegmentPassesDataWrapperBase, ISubGridCellSegmentPassesDataWrapper
    {
        public Cell_NonStatic[,] PassData = new Cell_NonStatic[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

        public SubGridCellSegmentPassesDataWrapper_NonStatic()
        {
        }

        public uint PassCount(uint X, uint Y)
        {
            return PassData[X, Y].PassCount;
        }

        public void AllocatePasses(uint X, uint Y, uint passCount)
        {
            PassData[X, Y].AllocatePasses(passCount);
        }

        public void AddPass(uint X, uint Y, CellPass pass, int position = -1)
        {
            PassData[X, Y].AddPass(pass, position);
        }

        public void ReplacePass(uint X, uint Y, int position, CellPass pass)
        {
            PassData[X, Y].ReplacePass(position, pass);
        }

        public CellPass ExtractCellPass(uint X, uint Y, int passNumber)
        {
            return PassData[X, Y].Passes[passNumber];
        }

        public bool LocateTime(uint X, uint Y, DateTime time, out int index)
        {
            return PassData[X, Y].LocateTime(time, out index);                    
        }

        public void Read(BinaryReader reader)
        {
            base.Read(reader, this);
        }

        public void Read(uint X, uint Y, BinaryReader reader)
        {
            uint passCount = PassCount(X, Y);
            for (uint cellPassIndex = 0; cellPassIndex < passCount; cellPassIndex++)
            {
                PassData[X, Y].Passes[cellPassIndex].Read(reader);
            }
        }

        public void Read(uint X, uint Y, uint passNumber, BinaryReader reader)
        {
            PassData[X, Y].Passes[passNumber].Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            base.Write(writer, this);
        }

        public void Write(uint X, uint Y, uint passNumber, BinaryWriter writer)
        {
            PassData[X, Y].Passes[passNumber].Write(writer);
        }

        public void Write(uint X, uint Y, BinaryWriter writer)
        {
            foreach (CellPass cellPass in PassData[X, Y].Passes)
            {
                cellPass.Write(writer);
            }
        }

        public float PassHeight(uint X, uint Y, uint passNumber)
        {
            return PassData[X, Y].Passes[passNumber].Height;
        }

        public DateTime PassTime(uint X, uint Y, uint passNumber)
        {
            return PassData[X, Y].Passes[passNumber].Time;
        }

        public void Integrate(uint X, uint Y, Cell_NonStatic source, uint StartIndex, uint EndIndex, out int AddedCount, out int ModifiedCount)
        {
            PassData[X, Y].Integrate(source, StartIndex, EndIndex, out AddedCount, out ModifiedCount);
        }

        public Cell_NonStatic Cell(uint X, uint Y)
        {
            return PassData[X, Y];
        }

        public CellPass Pass(uint X, uint Y, uint passIndex)
        {
            return PassData[X, Y].Passes[passIndex];
        }
    }

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
        /// Constructor that accepts a set of cell passes and creates the internal structures to hold them
        /// </summary>
        /// <param name="cellPassCount"></param>
        public SubGridCellSegmentPassesDataWrapper_Static(CellPass[,][] cellPasses)
        {
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
