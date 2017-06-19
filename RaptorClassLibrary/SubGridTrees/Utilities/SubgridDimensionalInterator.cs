using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.SubGridTrees.Utilities
{
    public static partial class SubGridUtilities
    {
        /// <summary>
        /// Iterates across the dimensional extent of a subgrid (ie: the [0..Dimension, 0..Dimension] indices) calling
        /// the supplied action functor.
        /// </summary>
        /// <param name="functor"></param>
        public static void SubGridDimensionalIterator(Action<uint, uint> functor)
        {
            for (uint I = 0; I < SubGridTree.SubGridTreeDimension - 1; I++)
            {
                for (uint J = 0; J < SubGridTree.SubGridTreeDimension - 1; J++)
                {
                    functor(I, J);
                }
            }
        }
    }
}
