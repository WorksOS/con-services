using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Client
{
    public class GenericClientLeafSubGrid<T> : ClientLeafSubGrid, ISubGrid, ILeafSubGrid, IClientLeafSubGrid
    {
        public T[,] Cells = null;

        /// <summary>
        /// Main constructor. Creates the local generic Items[,] array and delegates to base(...)
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public GenericClientLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)

        {
            Clear();
        }

        /// <summary>
        /// Iterates over all the cells in the leaf subgrid calling functor on each of them.
        /// Both non-null and null values are presented to functor. If functor returns false
        /// the ForEach terminates and returns false.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public bool ForEach(Func<T, bool> functor)
        {
            for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    if (!functor(Cells[I, J]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Iterates over all the cells in the leaf subgrid calling functor on each of them.
        /// Both non-null and null values are presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public void ForEach(Action<byte, byte, T> functor)
        {
            for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    functor(I, J, Cells[I, J]);
                }
            }
        }

        /// <summary>
        /// Iterates over all the cells in the leaf subgrid calling functor on each of them.
        /// Both non-null and null values are presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public void ForEach(Action<byte, byte> functor)
        {
            for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    functor(I, J);
                }
            }
        }

        public override void Clear()
        {
            // Recreate the array. .Net will initialise the memory used to zero's effecting the clear
            Cells = new T[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
        }

        /// <summary>
        /// Write the contents of leaf sub grid using the supplied formatter
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public override void Write(BinaryFormatter formatter, Stream stream)
        {
            base.Write(formatter, stream);

            formatter.Serialize(stream, Cells);
        }

        /// <summary>
        /// Fill the contents of the leaf sub grid reading the binary representation using the provided formatter
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public override void Read(BinaryFormatter formatter, Stream stream)
        {
            base.Read(formatter, stream);

            Cells = (T[,])formatter.Deserialize(stream);
        }

        /// <summary>
        /// Dumps the contents of this client leaf subgrid into the log in a human readable form
        /// </summary>
        public override void DumpToLog(string title)
        {
            // Implement when logging is set up
            // SIGLogMessage.PublishNoODS(Self, Format('Subgrid %s: %s', [Moniker, Title]), slmcDebug);
        }
    }
}
