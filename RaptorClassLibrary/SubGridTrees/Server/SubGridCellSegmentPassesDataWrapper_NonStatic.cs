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

        public void SetState(CellPass[,][] cellPasses)
        {
           PassData = new Cell_NonStatic[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

           SubGridUtilities.SubGridDimensionalIterator((x, y) => PassData[x, y].Passes = cellPasses[x, y]);
        }
    }
}
