using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    /// <summary>
    /// Factory that creates subgrid segments that contain collections of cell passes
    /// </summary>
    public class SubGridCellSegmentPassesDataWrapperFactory : ISubGridCellSegmentPassesDataWrapperFactory
    {
        private static SubGridCellSegmentPassesDataWrapperFactory instance = null;

        public ISubGridCellSegmentPassesDataWrapper NewWrapper()
        {
            //return new SubGridCellSegmentPassesDataWrapper_NonStatic();
            if (RaptorServerConfig.Instance().UseMutableCellPassSegments)
            {
                return new SubGridCellSegmentPassesDataWrapper_NonStatic();
            }
            else
            {
                return new SubGridCellSegmentPassesDataWrapper_Static();
            }
        }

        public static SubGridCellSegmentPassesDataWrapperFactory Instance()
        {
            if (instance == null)
            {
                instance = new SubGridCellSegmentPassesDataWrapperFactory();
            }

            return instance;
        }
    }
}
