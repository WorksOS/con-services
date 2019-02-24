﻿using System;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// Base class to hold common aspects of the segment implementation data wrapper between NonStatic, Static and 
    /// StaticCompressed implementations
    /// </summary>
    public class SubGridCellSegmentPassesDataWrapperBase
    {
        /// <summary>
        /// The count of cell passes residing in this sub grid segment
        /// </summary>
        public uint SegmentPassCount { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridCellSegmentPassesDataWrapperBase()
        {
        }

        /// <summary>
        /// Is the information contained in this wrapper mutable, or has is been converted to an immutable form from the base mutable data
        /// </summary>
        /// <returns></returns>
        public virtual bool IsImmutable() => false;
    }
}
