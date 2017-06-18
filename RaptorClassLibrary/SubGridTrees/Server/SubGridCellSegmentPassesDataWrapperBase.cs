using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
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
}
