using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
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
            bool exactMatch = PassData[X, Y].LocateTime(time, out index);

            if (!exactMatch)
            {
                // return previous cell pass as this is the one 'in effect' for the time being observed
                index--;
            }

            return exactMatch;
        }

        public void Read(BinaryReader reader)
        {
            int TotalPasses = reader.ReadInt32();
            int MaxPassCount = reader.ReadInt32();

            int[,] PassCounts = new int[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            int PassCounts_Size = PassCountSize.Calculate(MaxPassCount);

            SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                switch (PassCounts_Size)
                {
                    case 1: PassCounts[i, j] = reader.ReadByte(); break;
                    case 2: PassCounts[i, j] = reader.ReadInt16(); break;
                    case 3: PassCounts[i, j] = reader.ReadInt32(); break;
                    default:
                        throw new InvalidDataException(string.Format("Unknown PassCounts_Size {0}", PassCounts_Size));
                }
            });

            // Read all the cells from the stream
            SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                int PassCount_ = PassCounts[i, j];

                if (PassCount_ > 0)
                {
                    // TODO: Revisit static cell pass support for reading contexts
                    AllocatePasses(i, j, (uint)PassCount_);
                    Read(i, j, reader);

                    SegmentPassCount += PassCount_;
                };
            });
        }

        private void Read(uint X, uint Y, BinaryReader reader)
        {
            uint passCount = PassCount(X, Y);
            for (uint cellPassIndex = 0; cellPassIndex < passCount; cellPassIndex++)
            {
                PassData[X, Y].Passes[cellPassIndex].Read(reader);
            }
        }

        private void Read(uint X, uint Y, uint passNumber, BinaryReader reader)
        {
            PassData[X, Y].Passes[passNumber].Read(reader);
        }

        /// <summary>
        /// Calculate the total number of passes from all the cells present in this subgrid segment
        /// </summary>
        /// <param name="TotalPasses"></param>
        /// <param name="MaxPassCount"></param>
        private void CalculateTotalPasses(ref uint TotalPasses, ref uint MaxPassCount)
        {
            uint _TotalPasses = 0;
            uint _MaxPassCount = 0;

            SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                uint ThePassCount = PassCount(i, j);

                if (ThePassCount > _MaxPassCount)
                {
                    _MaxPassCount = ThePassCount;
                }

                _TotalPasses += ThePassCount;
            });

            TotalPasses = _TotalPasses;
            MaxPassCount = _MaxPassCount;
        }

        public void Write(BinaryWriter writer)
        {
            uint TotalPasses = 0;
            uint MaxPassCount = 0;

            CalculateTotalPasses(ref TotalPasses, ref MaxPassCount);

            writer.Write(TotalPasses);
            writer.Write(MaxPassCount);

            int PassCounts_Size = PassCountSize.Calculate((int)MaxPassCount);

            // Read all the cells from the stream
            SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                switch (PassCounts_Size)
                {
                    case 1: writer.Write((byte)PassCount(i, j)); break;
                    case 2: writer.Write((ushort)PassCount(i, j)); break;
                    case 3: writer.Write((int)PassCount(i, j)); break;
                    default:
                        throw new InvalidDataException(string.Format("Unknown PassCounts_Size: {0}", PassCounts_Size));
                }
            });

            // write all the cell passess to the stream, avoiding those cells that do not have any passes
            SubGridUtilities.SubGridDimensionalIterator((i, j) => 
            {
                if (PassCount(i, j) > 0)
                {
                    Write(i, j, writer);
                }
            });
        }

        private void Write(uint X, uint Y, uint passNumber, BinaryWriter writer)
        {
            PassData[X, Y].Passes[passNumber].Write(writer);
        }

        private void Write(uint X, uint Y, BinaryWriter writer)
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

        public void Integrate(uint X, uint Y, CellPass[] sourcePasses, uint StartIndex, uint EndIndex, out int AddedCount, out int ModifiedCount)
        {
            PassData[X, Y].Integrate(sourcePasses, StartIndex, EndIndex, out AddedCount, out ModifiedCount);
        }

        public CellPass[] ExtractCellPasses(uint X, uint Y)
        {
            return PassData[X, Y].Passes;
        }

        public CellPass Pass(uint X, uint Y, uint passIndex)
        {
            return PassData[X, Y].Passes[passIndex];
        }

        public void SetState(CellPass[,][] cellPasses)
        {
           PassData = new Cell_NonStatic[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

           SubGridUtilities.SubGridDimensionalIterator((x, y) => PassData[x, y].Passes = cellPasses[x, y]);
        }

        public CellPass[,][] GetState()
        {
            CellPass[,][] result = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension][];

            SubGridUtilities.SubGridDimensionalIterator((x, y) => result[x, y] = PassData[x, y].Passes);

            return result;
        }

    }
}
