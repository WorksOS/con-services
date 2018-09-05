using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// Supports conversion of stream representations of spatial and non-spatial information from mutable to immutable forms.
    /// Note: The transformation may be one way trapdoor function in the case of spatial data immutability conversions that
    /// result in the compressed form of the appropriate schema.
    /// </summary>
    public class MutabilityConverter : IMutabilityConverter
    {
      private static ILogger Log = Logging.Logger.CreateLogger(nameof(MutabilityConverter));

        /// <summary>
        /// Converts the structure of the global latext cells structure into an immutable form
        /// </summary>
        /// <returns></returns>
        public ISubGridCellLatestPassDataWrapper ConvertLatestPassesToImmutable(ISubGridCellLatestPassDataWrapper LatestPasses)
        {
            if (LatestPasses.IsImmutable())
            {
                return LatestPasses; // It is already immutable
            }

            SubGridCellLatestPassDataWrapper_NonStatic oldItem = LatestPasses as SubGridCellLatestPassDataWrapper_NonStatic;

            LatestPasses = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(false, true);
            LatestPasses.Assign(oldItem);

            (LatestPasses as SubGridCellLatestPassDataWrapper_StaticCompressed).PerformEncodingForInternalCache(oldItem.PassData);

            return LatestPasses;
        }

        /// <summary>
        /// Primary method for performing mutability conversion to immutable. It accepts a stream and an indication of the type of stream
        /// then delegates to conversion responsibility based on the stream type.
        /// </summary>
        /// <param name="streamType"></param>
        /// <param name="mutableStream"></param>
        /// <param name="immutableStream"></param>
        /// <returns></returns>
        public bool ConvertToImmutable(FileSystemStreamType streamType, MemoryStream mutableStream, out MemoryStream immutableStream)
        {
            immutableStream = null;

            switch(streamType)
            {
                case FileSystemStreamType.SubGridDirectory:
                    {
                        return ConvertSubgridDirectoryToImmutable(mutableStream, out immutableStream);
                    }
                case FileSystemStreamType.SubGridSegment:
                    {
                        return ConvertSubgridSegmentToImmutable(mutableStream, out immutableStream);
                    }
                case FileSystemStreamType.Events:
                    {
                        return ConvertEventListToImmutable(mutableStream, out immutableStream);
                    }
                default:
                    {
                        // EG: Subgrid existence map etc
                        immutableStream = mutableStream;
                        return true;
                    }
            }
        }

        /// <summary>
        /// Converts a subgrid directory into its immutable form
        /// </summary>
        /// <param name="mutableStream"></param>
        /// <param name="immutableStream"></param>
        /// <returns></returns>
        public bool ConvertSubgridDirectoryToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream)
        {
            try
            {
                // create a leaf to contain the mutable directory (and ensure the global latest cells is the mutable variety)
                IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(null, null, SubGridTreeConsts.SubGridTreeLevels)
                {
                    Directory =
                    {
                        GlobalLatestCells = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(true, false)
                    }
                };


                // Load the mutable stream of information
                mutableStream.Position = 0;
                leaf.LoadDirectoryFromStream(mutableStream);

                leaf.Directory.GlobalLatestCells = ConvertLatestPassesToImmutable(leaf.Directory.GlobalLatestCells);

                immutableStream = new MemoryStream();
                leaf.SaveDirectoryToStream(immutableStream);

                return true;
            }
            catch (Exception e)
            {
                immutableStream = null;

                Log.LogError($"Exception in conversion of subgrid directory mutable data to immutable schema: {e}");
                return false;
            }
        }

        /// <summary>
        /// Converts a subgrid segment into its immutable form
        /// </summary>
        /// <param name="mutableStream"></param>
        /// <param name="immutableStream"></param>
        /// <returns></returns>
        public bool ConvertSubgridSegmentToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream)
        {
            try
            {
                // Read in the subgrid segment from the mutable stream
                SubGridCellPassesDataSegment segment = new SubGridCellPassesDataSegment
                    (SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(true, false),
                     SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(true, false));

                mutableStream.Position = 0;
                using (var reader = new BinaryReader(mutableStream, Encoding.UTF8, true))
                {
                    segment.Read(reader, true, true);
                }

                // Convert to the immutable form
                segment.LatestPasses = ConvertLatestPassesToImmutable(segment.LatestPasses);

                ISubGridCellSegmentPassesDataWrapper mutablePassesData = segment.PassesData;

                segment.PassesData = SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(false, true);
                segment.PassesData.SetState(mutablePassesData.GetState());

                // Write out the segment to the immutable stream
                immutableStream = new MemoryStream();
                using (var writer = new BinaryWriter(immutableStream, Encoding.UTF8, true))
                {
                    segment.Write(writer);
                }

                return true;
            }
            catch (Exception e)
            {
                immutableStream = null;

                Log.LogError($"Exception in conversion of subgrid segment mutable data to immutable schema: {e}");
                return false;
            }
        }

        /// <summary>
        /// Convert an event list to it's immutable form. 
        /// Currently this is a no-op - the original stream is returned as there is not yet an immutable description for events
        /// </summary>
        /// <param name="mutableStream"></param>
        /// <param name="immutableStream"></param>
        /// <returns></returns>
        public bool ConvertEventListToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream)
        {
            immutableStream = mutableStream;

            return true;
        }
    }
}
