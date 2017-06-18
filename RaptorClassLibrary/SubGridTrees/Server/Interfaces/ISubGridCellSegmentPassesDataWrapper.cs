using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces
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
}
