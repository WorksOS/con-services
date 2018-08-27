using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Core.Utilities
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
            for (uint I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
            {
                for (uint J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
                {
                    functor(I, J);
                }
            }
        }
    }
}
